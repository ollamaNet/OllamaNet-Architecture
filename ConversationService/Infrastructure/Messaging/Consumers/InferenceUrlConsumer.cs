using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ConversationService.Infrastructure.Configuration;
using ConversationService.Infrastructure.Messaging.Interfaces;
using ConversationService.Infrastructure.Messaging.Models;
using ConversationService.Infrastructure.Messaging.Options;
using ConversationService.Infrastructure.Messaging.Resilience;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ConversationService.Infrastructure.Messaging.Consumers;

public class InferenceUrlConsumer : BackgroundService, IMessageConsumer
{
    private readonly IInferenceEngineConfiguration _configuration;
    private readonly ILogger<InferenceUrlConsumer> _logger;
    private readonly RabbitMQOptions _options;
    private readonly IAsyncPolicy _retryPolicy;
    private readonly RabbitMQResiliencePolicies _resiliencePolicies;
    private IConnection _connection;
    private IModel _channel;
    private bool _isConnected;

    public bool IsConnected => _isConnected && _connection?.IsOpen == true;

    public InferenceUrlConsumer(
        IInferenceEngineConfiguration configuration,
        IOptions<RabbitMQOptions> options,
        RabbitMQResiliencePolicies resiliencePolicies,
        ILogger<InferenceUrlConsumer> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _options = options.Value;
        _retryPolicy = resiliencePolicies.RetryPolicy;
        _resiliencePolicies = resiliencePolicies;
        _isConnected = false;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _retryPolicy.ExecuteAsync(async () => {
                await ConnectToRabbitMQ();
                _isConnected = true;
            });

            while (!stoppingToken.IsCancellationRequested)
            {
                if (!_resiliencePolicies.IsConnected)
                {
                    _isConnected = false;
                    _logger.LogWarning("Circuit is open. Waiting before retrying RabbitMQ connection");
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                    continue;
                }

                if (_connection == null || !_connection.IsOpen)
                {
                    _isConnected = false;
                    await _retryPolicy.ExecuteAsync(async () => {
                        await ConnectToRabbitMQ();
                        _isConnected = true;
                    });
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            _isConnected = false;
            _logger.LogError(ex, "Error in InferenceUrlConsumer");
        }
    }

    private async Task ConnectToRabbitMQ()
    {
        _logger.LogInformation("Connecting to RabbitMQ at {Host}:{Port}", _options.HostName, _options.Port);
        
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(
            exchange: _options.Exchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);

        _channel.QueueDeclare(
            queue: _options.InferenceUrlQueue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        _channel.QueueBind(
            queue: _options.InferenceUrlQueue,
            exchange: _options.Exchange,
            routingKey: _options.InferenceUrlRoutingKey);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = JsonSerializer.Deserialize<InferenceUrlUpdateMessage>(body);

                if (message != null && !string.IsNullOrWhiteSpace(message.NewUrl))
                {
                    _logger.LogInformation("Received URL update message: {Url} (ServiceId: {ServiceId})", 
                        message.NewUrl, message.ServiceId);
                    await _configuration.UpdateBaseUrl(message.NewUrl);
                }

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing inference URL update message");
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume(
            queue: _options.InferenceUrlQueue,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation("Connected to RabbitMQ and consuming from {Queue}", _options.InferenceUrlQueue);
        
        await Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping InferenceUrlConsumer");
        _isConnected = false;
        _channel?.Close();
        _connection?.Close();
        await base.StopAsync(cancellationToken);
    }
} 