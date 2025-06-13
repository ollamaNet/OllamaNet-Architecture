# InferenceEngine Service Discovery Using RabbitMQ

## Overview

This document outlines the implementation plan for dynamically updating the InferenceEngine URL in ConversationService using RabbitMQ. The InferenceEngine service will publish URL updates to a message queue whenever its address changes (approximately every 3 hours or on restart), and ConversationService will consume these messages to update its configuration.

## Requirements

- Dynamic updates of InferenceEngine URL without service restart
- Persistence of URL across ConversationService restarts
- Validation of URL format (should follow "https://d0dc-34-142-209-71.ngrok-free.app/" pattern)
- Fallback to appsettings.json value if RabbitMQ is unavailable
- Support for multiple ConversationService instances
- Resilient error handling with Polly
- Simple health monitoring

## Architecture

### Component Diagram

```
┌─────────────────┐     ┌──────────┐     ┌───────────────────┐
│ InferenceEngine │────▶│ RabbitMQ │────▶│ ConversationService│
└─────────────────┘     └──────────┘     └───────────────────┘
        │                                          │
        │                                          │
        └──────────────────┐               ┌───────┘
                           ▼               ▼
                        ┌──────────────────────┐
                        │        Redis         │
                        └──────────────────────┘
```

### Key Components

1. **Message Publisher** (InferenceEngine side)
   - Publishes URL updates to RabbitMQ

2. **Message Consumer** (ConversationService)
   - Background service that listens for URL updates
   - Updates configuration and Redis cache on receipt

3. **Configuration Service**
   - Manages the current URL with Redis persistence
   - Provides URL to InferenceEngineConnector
   - Exposes URL change events

4. **Resilience Layer**
   - Polly-based policies for RabbitMQ connection retries
   - Circuit breaker for fault isolation

## Implementation Details

### 1. Directory Structure

```
ConversationService/
├── Infrastructure/
│   ├── Integration/
│   │   ├── InferenceEngine/    <-- Renamed from OllamaConnector
│   │   │   ├── InferenceEngineConnector.cs
│   │   │   └── IInferenceEngineConnector.cs
│   │   ├── Messaging/              <-- New component
│   │   │   ├── Options/
│   │   │   │   └── RabbitMQOptions.cs
│   │   │   ├── Models/
│   │   │   │   └── InferenceUrlUpdateMessage.cs
│   │   │   ├── Consumers/
│   │   │   │   └── InferenceUrlConsumer.cs
│   │   │   ├── Resilience/
│   │   │   │   └── RabbitMQResiliencePolicies.cs
│   │   │   ├── Validators/
│   │   │   │   └── UrlValidator.cs
│   │   │   ├── Interfaces/
│   │   │   │   └── IMessageConsumer.cs
│   │   │   └── Extensions/
│   │   │       └── MessagingServiceExtensions.cs
│   │   └── Configuration/          <-- New component
│   │       └── InferenceEngineConfiguration.cs
│   └── Configuration/          <-- New component
│       └── InferenceEngineConfiguration.cs
```

### 2. Message Schema

```csharp
public class InferenceUrlUpdateMessage
{
    public string NewUrl { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string ServiceId { get; set; } = "inference-engine";
    public string Version { get; set; } = "1.0";
}
```

### 3. URL Configuration Service

```csharp
public interface IInferenceEngineConfiguration
{
    string GetBaseUrl();
    Task UpdateBaseUrl(string newUrl);
    event Action<string> BaseUrlChanged;
}

public class InferenceEngineConfiguration : IInferenceEngineConfiguration
{
    private string _currentBaseUrl;
    private readonly ILogger<InferenceEngineConfiguration> _logger;
    private readonly IRedisCacheService _redisCacheService;
    private readonly IUrlValidator _urlValidator;
    private const string CACHE_KEY = "InferenceEngine:BaseUrl";

    public event Action<string> BaseUrlChanged;

    public InferenceEngineConfiguration(
        IConfiguration configuration, 
        ILogger<InferenceEngineConfiguration> logger,
        IRedisCacheService redisCacheService,
        IUrlValidator urlValidator)
    {
        _logger = logger;
        _redisCacheService = redisCacheService;
        _urlValidator = urlValidator;
        
        // Try to get from cache first, fall back to config
        var cachedUrl = _redisCacheService.GetStringAsync(CACHE_KEY).Result;
        _currentBaseUrl = cachedUrl ?? configuration["InferenceEngine:BaseUrl"];
    }

    public string GetBaseUrl() => _currentBaseUrl;

    public async Task UpdateBaseUrl(string newUrl)
    {
        if (string.IsNullOrEmpty(newUrl) || _currentBaseUrl == newUrl)
            return;
            
        if (!_urlValidator.IsValid(newUrl))
        {
            _logger.LogWarning("Invalid URL format received: {NewUrl}", newUrl);
            return;
        }
            
        _currentBaseUrl = newUrl;
        await _redisCacheService.SetStringAsync(CACHE_KEY, newUrl);
        _logger.LogInformation("InferenceEngine BaseUrl updated to: {NewUrl}", newUrl);
        
        BaseUrlChanged?.Invoke(newUrl);
    }
}
```

### 4. URL Validator

```csharp
public interface IUrlValidator
{
    bool IsValid(string url);
}

public class UrlValidator : IUrlValidator
{
    private static readonly Regex UrlRegex = new Regex(
        @"^@https://[a-zA-Z0-9][-a-zA-Z0-9]*\.ngrok-free\.app/?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public bool IsValid(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;
            
        return UrlRegex.IsMatch(url);
    }
}
```

### 5. RabbitMQ Consumer with Polly Resilience

```csharp
public class InferenceUrlConsumer : BackgroundService
{
    private readonly IInferenceEngineConfiguration _configuration;
    private readonly ILogger<InferenceUrlConsumer> _logger;
    private readonly RabbitMQOptions _options;
    private readonly IAsyncPolicy _retryPolicy;
    private readonly ICircuitBreakerPolicy _circuitBreakerPolicy;
    private IConnection _connection;
    private IModel _channel;

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
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _retryPolicy.ExecuteAsync(async () => {
                await ConnectToRabbitMQ();
            });

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_circuitBreakerPolicy.CircuitState == CircuitState.Open)
                {
                    _logger.LogWarning("Circuit is open. Waiting before retrying RabbitMQ connection");
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                    continue;
                }

                if (_connection == null || !_connection.IsOpen)
                {
                    await _retryPolicy.ExecuteAsync(async () => {
                        await ConnectToRabbitMQ();
                    });
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            _logger.LogError(ex, "Error in InferenceUrlConsumer");
        }
    }

    private async Task ConnectToRabbitMQ()
    {
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
                    await _configuration.UpdateBaseUrl(message.NewUrl);
                    _logger.LogInformation("Updated inference engine URL to {Url} from message", message.NewUrl);
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
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.Close();
        _connection?.Close();
        await base.StopAsync(cancellationToken);
    }
}
```

### 6. Resilience Policies

```csharp
public class RabbitMQResiliencePolicies
{
    public IAsyncPolicy RetryPolicy { get; }
    public ICircuitBreakerPolicy CircuitBreakerPolicy { get; }

    public RabbitMQResiliencePolicies(ILogger<RabbitMQResiliencePolicies> logger)
    {
        RetryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                5,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    logger.LogWarning(exception, 
                        "Error connecting to RabbitMQ. Retry attempt {RetryCount} after {RetryTimeSpan}s", 
                        retryCount, timeSpan.TotalSeconds);
                });

        CircuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: (ex, breakDelay) =>
                {
                    logger.LogError(ex, "Circuit breaker opened for {BreakDelay}s", breakDelay.TotalSeconds);
                },
                onReset: () =>
                {
                    logger.LogInformation("Circuit breaker reset");
                },
                onHalfOpen: () =>
                {
                    logger.LogInformation("Circuit breaker half-open");
                });
    }
}
```

### 7. RabbitMQ Options

```csharp
public class RabbitMQOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string Exchange { get; set; } = "service-discovery";
    public string InferenceUrlQueue { get; set; } = "inference-url-updates";
    public string InferenceUrlRoutingKey { get; set; } = "inference.url.changed";
}
```

### 8. InferenceEngineConnector (Renamed from OllamaConnector)

```csharp
public class InferenceEngineConnector : IInferenceEngineConnector
{
    private readonly IInferenceEngineConfiguration _configuration;
    private readonly ILogger<InferenceEngineConnector> _logger;
    // Other dependencies
    private HttpClient _httpClient;

    public InferenceEngineConnector(
        IInferenceEngineConfiguration configuration,
        ILogger<InferenceEngineConnector> logger,
        /* other dependencies */)
    {
        _configuration = configuration;
        _logger = logger;
        // Initialize other dependencies
        
        // Create initial client
        _httpClient = CreateClient();
        
        // Subscribe to URL changes
        _configuration.BaseUrlChanged += OnBaseUrlChanged;
    }

    private void OnBaseUrlChanged(string newUrl)
    {
        _logger.LogInformation("Updating inference engine client for new URL: {Url}", newUrl);
        // Dispose old client and create new one
        var oldClient = _httpClient;
        _httpClient = CreateClient();
        oldClient?.Dispose();
    }

    private HttpClient CreateClient()
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri(_configuration.GetBaseUrl().TrimStart('@'))
        };
        // Configure client
        return client;
    }

    // Implement other methods using _httpClient
}
```

## Configuration

### appsettings.json

```json
{
  "InferenceEngine": {
    "BaseUrl": "@https://default-inference-engine-url.ngrok-free.app/"
  },
  "RabbitMQ": {
    "HostName": "YOUR_RABBITMQ_HOST",
    "Port": 5672,
    "UserName": "YOUR_USERNAME",
    "Password": "YOUR_PASSWORD",
    "VirtualHost": "/",
    "Exchange": "service-discovery",
    "InferenceUrlQueue": "inference-url-updates",
    "InferenceUrlRoutingKey": "inference.url.changed"
  }
}
```

### Dependency Injection

```csharp
public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
{
    // Register options
    services.Configure<RabbitMQOptions>(configuration.GetSection("RabbitMQ"));
    
    // Register resilience policies
    services.AddSingleton<RabbitMQResiliencePolicies>();
    
    // Register URL validation
    services.AddSingleton<IUrlValidator, UrlValidator>();
    
    // Register configuration
    services.AddSingleton<IInferenceEngineConfiguration, InferenceEngineConfiguration>();
    
    // Register consumer
    services.AddHostedService<InferenceUrlConsumer>();
    
    return services;
}

// Update existing service registration
public static IServiceCollection AddInferenceEngine(this IServiceCollection services)
{
    services.AddSingleton<IInferenceEngineConnector, InferenceEngineConnector>();
    
    return services;
}
```

## Health Monitoring

A simple health monitoring approach for the RabbitMQ connection:

```csharp
[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly InferenceUrlConsumer _consumer;
    private readonly IInferenceEngineConnector _connector;
    
    public HealthController(
        InferenceUrlConsumer consumer,
        IInferenceEngineConnector connector)
    {
        _consumer = consumer;
        _connector = connector;
    }
    
    [HttpGet("rabbitmq")]
    public IActionResult CheckRabbitMQHealth()
    {
        var status = _consumer.IsConnected ? "Connected" : "Disconnected";
        return Ok(new { Status = status });
    }
    
    [HttpGet("inference-engine")]
    public IActionResult CheckInferenceEngineHealth()
    {
        var url = _connector.GetBaseUrl();
        return Ok(new { Url = url });
    }
}
```

## Security Considerations

### RabbitMQ Authentication Options

1. **Username/Password**:
   - Standard authentication method
   - Credentials stored in appsettings.json or environment variables
   - Example: `amqp://username:password@host:port/vhost`

2. **Client Certificates**:
   - TLS client certificates for mutual authentication
   - More secure than username/password
   - Requires certificate management

3. **OAuth 2.0**:
   - Token-based authentication
   - Works well with existing OAuth infrastructure
   - More complex to set up

4. **SASL External**:
   - Uses external authentication mechanisms
   - Can integrate with LDAP or other directory services

For the current implementation, username/password authentication is used, but credentials should be stored securely (e.g., using Azure Key Vault or environment variables in production).

## Migration Steps

1. Create new directory structure for messaging and configuration
2. Add new NuGet packages:
   - RabbitMQ.Client
   - Polly
3. Implement InferenceEngineConfiguration with Redis persistence
4. Implement RabbitMQ consumer with Polly resilience
5. Rename OllamaConnector to InferenceEngineConnector and update
6. Update dependency injection in ServiceExtensions.cs
7. Update appsettings.json with new InferenceEngine and RabbitMQ sections
8. Update all references in other services

## NuGet Package Dependencies

- RabbitMQ.Client
- Polly
- Polly.Extensions.Http
- Microsoft.Extensions.Http.Polly
- System.Text.Json

## Deployment Considerations

### Cloud RabbitMQ Provider Options

1. **CloudAMQP**:
   - Managed RabbitMQ service
   - Multiple pricing tiers
   - Easy setup and management

2. **Azure Service Bus**:
   - Native Azure service
   - AMQP protocol support
   - Integration with other Azure services

3. **Amazon MQ**:
   - Managed RabbitMQ service on AWS
   - Integration with AWS ecosystem
   - Multiple availability zones

### Docker Container Deployment

1. **Official RabbitMQ Image**:
   ```bash
   docker run -d --name rabbitmq \
     -p 5672:5672 -p 15672:15672 \
     -e RABBITMQ_DEFAULT_USER=username \
     -e RABBITMQ_DEFAULT_PASS=password \
     rabbitmq:3-management
   ```

2. **Docker Compose**:
   ```yaml
   version: '3'
   services:
     rabbitmq:
       image: rabbitmq:3-management
       ports:
         - "5672:5672"
         - "15672:15672"
       environment:
         - RABBITMQ_DEFAULT_USER=username
         - RABBITMQ_DEFAULT_PASS=password
       volumes:
         - rabbitmq_data:/var/lib/rabbitmq
   volumes:
     rabbitmq_data:
   ```

## Next Steps

1. Determine specific RabbitMQ provider and credentials
2. Implement the InferenceEngine URL update publisher on the inference service side
3. Update all references to OllamaConnector throughout the codebase
4. Set up monitoring and alerting for RabbitMQ connection status
5. Test with various failure scenarios to ensure resilience
6. Document the final implementation for team reference
