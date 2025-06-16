using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using AdminService.Infrastructure.Configuration;
using AdminService.Infrastructure.Configuration.Options;
using AdminService.Infrastructure.Messaging.Models;
using AdminService.Infrastructure.Messaging.Resilience;

namespace AdminService.Infrastructure.Messaging.Consumers
{
    /// <summary>
    /// Background service that consumes Inference Engine URL update messages from RabbitMQ
    /// </summary>
    public class InferenceUrlConsumer : BackgroundService
    {
        private readonly IInferenceEngineConfiguration _configuration;
        private readonly ILogger<InferenceUrlConsumer> _logger;
        private readonly RabbitMQOptions _options;
        private readonly IAsyncPolicy _retryPolicy;
        private readonly ICircuitBreakerPolicy _circuitBreakerPolicy;
        private IConnection _connection;
        private IModel _channel;
        private bool _isConnected;

        /// <summary>
        /// Gets whether the consumer is connected to RabbitMQ
        /// </summary>
        public bool IsConnected => _isConnected && _connection?.IsOpen == true;

        /// <summary>
        /// Constructor for InferenceUrlConsumer
        /// </summary>
        /// <param name="configuration">Inference engine configuration</param>
        /// <param name="options">RabbitMQ options</param>
        /// <param name="resiliencePolicies">RabbitMQ resilience policies</param>
        /// <param name="logger">Logger</param>
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
            _circuitBreakerPolicy = resiliencePolicies.CircuitBreakerPolicy;
            _isConnected = false;
        }

        /// <inheritdoc />
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
                    if (_circuitBreakerPolicy.CircuitState == CircuitState.Open)
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

        /// <inheritdoc />
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping InferenceUrlConsumer");
            _isConnected = false;
            _channel?.Close();
            _connection?.Close();
            await base.StopAsync(cancellationToken);
        }
    }
}
 