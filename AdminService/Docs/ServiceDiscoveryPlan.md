# AdminService: Inference Engine Service Discovery Implementation Plan

## Overview

This document outlines the implementation plan for dynamically updating the Inference Engine URL in AdminService using RabbitMQ. The AdminService will subscribe to the same message queue that ConversationService uses, allowing both services to receive URL updates whenever the Inference Engine's address changes.

## Requirements

- Subscribe to existing Inference Engine URL updates via RabbitMQ
- Dynamic updates of Inference Engine URL without service restart
- Persistence of URL across AdminService restarts using Redis (to be implemented)
- Validation of URL format (should follow "https://d0dc-34-142-209-71.ngrok-free.app/" pattern)
- Fallback to appsettings.json value if RabbitMQ is unavailable
- Resilient error handling with Polly
- Direct integration with Ollama API using the InferenceEngineConnector

## Architecture

### Component Diagram

```
┌─────────────────┐     ┌──────────┐     ┌───────────────────┐
│ InferenceEngine │────▶│ RabbitMQ │────▶│ AdminService      │
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

1. **Message Consumer** (AdminService)
   - Background service that listens for URL updates
   - Updates configuration and Redis cache on receipt

2. **Configuration Service**
   - Manages the current URL with Redis persistence
   - Provides URL to InferenceEngineConnector
   - Exposes URL change events

3. **InferenceEngineConnector**
   - Replaces the existing OllamaConnector
   - Directly integrates with Ollama API
   - Updates connection when URL changes

4. **Resilience Layer**
   - Polly-based policies for RabbitMQ connection retries
   - Circuit breaker for fault isolation

## Implementation Details

### 1. Directory Structure

```
AdminService/
├── Connectors/
│   ├── InferenceEngineConnector.cs  <-- Replaces OllamaConnector
│   ├── IInferenceEngineConnector.cs <-- Replaces IOllamaConnector
├── Infrastructure/
│   ├── Integration/
│   │   ├── Messaging/         <-- New component
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
│   │   │   └── Extensions/
│   │   │       └── MessagingServiceExtensions.cs
│   │   └── Configuration/     <-- New component
│   │       └── InferenceEngineConfiguration.cs
│   ├── Caching/               <-- New component
│   │   ├── IRedisCacheService.cs
│   │   ├── RedisCacheService.cs
│   │   └── CacheKeys.cs
```

### 2. Message Schema (Same as ConversationService)

```csharp
public class InferenceUrlUpdateMessage
{
    public string NewUrl { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string ServiceId { get; set; } = "inference-engine";
    public string Version { get; set; } = "1.0";
}
```

### 3. Redis Cache Service (New Component)

```csharp
public interface IRedisCacheService
{
    Task<string> GetStringAsync(string key);
    Task SetStringAsync(string key, string value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
}

public class RedisCacheService : IRedisCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(
        IConfiguration configuration,
        ILogger<RedisCacheService> logger)
    {
        _logger = logger;
        
        var redisConnectionString = configuration["Redis:ConnectionString"];
        if (string.IsNullOrEmpty(redisConnectionString))
        {
            _logger.LogWarning("Redis connection string is not configured. Using empty connection.");
            _redis = ConnectionMultiplexer.Connect("localhost");
            return;
        }
        
        try
        {
            _redis = ConnectionMultiplexer.Connect(redisConnectionString);
            _logger.LogInformation("Connected to Redis");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to Redis");
            _redis = ConnectionMultiplexer.Connect("localhost");
        }
    }

    public async Task<string> GetStringAsync(string key)
    {
        try
        {
            var db = _redis.GetDatabase();
            return await db.StringGetAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting string from Redis for key: {Key}", key);
            return null;
        }
    }

    public async Task SetStringAsync(string key, string value, TimeSpan? expiration = null)
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.StringSetAsync(key, value, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting string in Redis for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing key from Redis: {Key}", key);
        }
    }
}
```

### 4. URL Configuration Service

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
        _currentBaseUrl = cachedUrl ?? configuration["OllamaApi:BaseUrl"];
        
        if (string.IsNullOrEmpty(_currentBaseUrl))
        {
            _currentBaseUrl = "http://localhost:11434";
            _logger.LogWarning("No InferenceEngine BaseUrl found in cache or configuration. Using default: {DefaultUrl}", _currentBaseUrl);
        }
        
        _logger.LogInformation("InferenceEngine configured with URL: {Url}", _currentBaseUrl);
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

### 5. URL Validator

```csharp
public interface IUrlValidator
{
    bool IsValid(string url);
}

public class UrlValidator : IUrlValidator
{
    private static readonly Regex UrlRegex = new Regex(
        @"^https://[a-zA-Z0-9][-a-zA-Z0-9]*\.ngrok-free\.app/?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public bool IsValid(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;
            
        return UrlRegex.IsMatch(url);
    }
}
```

### 6. InferenceEngineConnector Implementation

```csharp
public interface IInferenceEngineConnector
{
    IReadOnlyDictionary<string, object?> Attributes { get; }
    Task<IEnumerable<Model>> GetInstalledModels();
    Task<IEnumerable<Model>> GetInstalledModelsPaged(int pageNumber, int pageSize);
    Task<ShowModelResponse> GetModelInfo(string modelName);
    Task<string> RemoveModel(string modelName);
    IAsyncEnumerable<InstallProgressInfo> PullModelAsync(string modelName);
    string GetBaseUrl();
}

public class InferenceEngineConnector : IInferenceEngineConnector
{
    private readonly IInferenceEngineConfiguration _configuration;
    private readonly ILogger<InferenceEngineConnector> _logger;
    private IOllamaApiClient _ollamaApiClient;
    private readonly object _lockObject = new object();

    public InferenceEngineConnector(
        IInferenceEngineConfiguration configuration,
        ILogger<InferenceEngineConnector> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        // Create initial client
        _ollamaApiClient = new OllamaApiClient(_configuration.GetBaseUrl());
        
        // Subscribe to URL changes
        _configuration.BaseUrlChanged += OnBaseUrlChanged;
    }

    private void OnBaseUrlChanged(string newUrl)
    {
        _logger.LogInformation("Updating OllamaApiClient for new URL: {Url}", newUrl);
        lock (_lockObject)
        {
            // Create new client with the updated URL
            _ollamaApiClient = new OllamaApiClient(newUrl);
        }
    }

    public string GetBaseUrl() => _configuration.GetBaseUrl();

    public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

    public async Task<IEnumerable<Model>> GetInstalledModels()
    {
        lock (_lockObject)
        {
            return _ollamaApiClient.ListLocalModelsAsync();
        }
    }

    public async Task<IEnumerable<Model>> GetInstalledModelsPaged(int pageNumber, int pageSize)
    {
        var response = await _ollamaApiClient.ListLocalModelsAsync();

        // Ensure pageNumber and pageSize are valid
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        // Calculate the items to skip and take for pagination
        var skip = (pageNumber - 1) * pageSize;
        var pagedResponse = response.Skip(skip).Take(pageSize);

        return pagedResponse;
    }

    public async Task<ShowModelResponse> GetModelInfo(string modelName)
    {
        lock (_lockObject)
        {
            return _ollamaApiClient.ShowModelAsync(modelName);
        }
    }

    public async Task<string> RemoveModel(string modelName)
    {
        await _ollamaApiClient.DeleteModelAsync(modelName);

        var models = await _ollamaApiClient.ListLocalModelsAsync();
        var modelExists = models.Any(m => m.Name == modelName);

        return modelExists ? "Model not removed successfully" : "Model removed successfully";
    }

    public async IAsyncEnumerable<InstallProgressInfo> PullModelAsync(string modelName)
    {
        if (string.IsNullOrWhiteSpace(modelName))
            throw new ArgumentException("Model name cannot be empty.", nameof(modelName));

        await foreach (var response in _ollamaApiClient.PullModelAsync(modelName))
        {
            yield return new InstallProgressInfo
            {
                Status = response.Status,
                Digest = response.Digest,
                Total = response.Total,
                Completed = response.Completed,
                Progress = response.Total > 0
                    ? (double)response.Completed / response.Total * 100
                    : 0
            };
        }
    }
}
```

### 7. RabbitMQ Consumer with Polly Resilience

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
        _circuitBreakerPolicy = resiliencePolicies.CircuitBreakerPolicy;
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

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping InferenceUrlConsumer");
        _isConnected = false;
        _channel?.Close();
        _connection?.Close();
        await base.StopAsync(cancellationToken);
    }
}
```

### 8. Resilience Policies

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

### 9. RabbitMQ Options

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

## Configuration

### appsettings.json

```json
{
  "OllamaApi": {
    "BaseUrl": "http://localhost:11434"
  },
  "Redis": {
    "ConnectionString": "YOUR_REDIS_CONNECTION_STRING"
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
// Add these new extension methods to ServiceExtensions.cs

public static IServiceCollection AddRedisCaching(this IServiceCollection services, IConfiguration configuration)
{
    services.AddSingleton<IRedisCacheService, RedisCacheService>();
    
    return services;
}

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

// Update InferenceEngineConnector registration
public static IServiceCollection AddInferenceEngineConnector(this IServiceCollection services)
{
    services.AddSingleton<IInferenceEngineConnector, InferenceEngineConnector>();
    
    return services;
}
```

### Program.cs Updates

```csharp
// Find where services.AddSingleton<IOllamaConnector, OllamaConnector>() is registered
// and replace with:

// Add Redis caching for service discovery
services.AddRedisCaching(Configuration);

// Add messaging for service discovery
services.AddMessaging(Configuration);

// Add InferenceEngineConnector
services.AddInferenceEngineConnector();
```

## Integration with Health Monitoring

Add RabbitMQ and Inference Engine URL health checks:

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

## NuGet Package Dependencies

- RabbitMQ.Client
- Polly
- Polly.Extensions.Http
- StackExchange.Redis
- System.Text.Json

## Implementation Steps

1. Create the new directory structure for messaging and caching components
2. Add required NuGet packages to the project
3. Implement the Redis cache service for URL persistence
4. Implement the URL configuration service
5. Create the InferenceEngineConnector implementation
6. Implement the RabbitMQ consumer with Polly resilience
7. Update interface references from IOllamaConnector to IInferenceEngineConnector
8. Add service extensions for dependency injection
9. Update appsettings.json with RabbitMQ and Redis configuration
10. Register the new components in Program.cs
11. Add health monitoring endpoints

## Testing Plan

1. Verify RabbitMQ connection with debug logging
2. Test URL update messages through the RabbitMQ queue
3. Verify the InferenceEngineConnector uses the updated URL for making API calls
4. Test fallback to appsettings.json when RabbitMQ is unavailable
5. Verify URL persistence in Redis across service restarts
6. Test health endpoint responses
7. Verify model operations with dynamically updated URLs
