# AdminService: Inference Engine Service Discovery Implementation Plan

## Overview

This document provides the detailed implementation plan for integrating service discovery in AdminService. It outlines the specific code implementations, dependencies, and steps required to dynamically update the Inference Engine URL using RabbitMQ and Redis caching.

## Phased Implementation Approach

### Phase 1: Infrastructure Setup (Days 1-2)
1. Create directory structure
2. Add required NuGet packages
3. Implement Redis caching components
4. Implement configuration options classes

### Phase 2: Messaging Implementation (Days 3-4)
1. Implement message schemas
2. Implement RabbitMQ consumer
3. Implement resilience policies
4. Configure RabbitMQ options

### Phase 3: Connector Transition (Days 5-6)
1. Create InferenceEngineConnector interface and implementation
2. Keep OllamaConnector temporarily for backward compatibility
3. Update dependency injection to support both connectors
4. Gradually migrate service references to the new connector

### Phase 4: Service Integration (Days 7-8)
1. Update InferenceOperationsService to use the new connector
2. Add health monitoring endpoints
3. Update configurations

### Phase 5: Testing and Cleanup (Days 9-10)
1. Comprehensive testing of the new implementation
2. Remove deprecated OllamaConnector when transition is complete
3. Document the new architecture

## Implementation Components

### 1. Message Schema

```csharp
public class InferenceUrlUpdateMessage
{
    public string NewUrl { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string ServiceId { get; set; } = "inference-engine";
    public string Version { get; set; } = "1.0";
}
```

### 2. Redis Cache Service

```csharp
public interface ICacheManager
{
    Task<string> GetStringAsync(string key);
    Task SetStringAsync(string key, string value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
}

public class RedisCacheService : ICacheManager
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(
        IConfiguration configuration,
        ILogger<RedisCacheService> logger)
    {
        _logger = logger;
        
        var redisConnectionString = configuration.GetConnectionString("Redis");
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

### 3. Cache Keys

```csharp
public static class CacheKeys
{
    public const string InferenceEngineUrl = "InferenceEngine:BaseUrl";
    
    // Add other cache keys as needed
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
    private readonly ICacheManager _cacheManager;
    private readonly IUrlValidator _urlValidator;

    public event Action<string> BaseUrlChanged;

    public InferenceEngineConfiguration(
        IConfiguration configuration, 
        ILogger<InferenceEngineConfiguration> logger,
        ICacheManager cacheManager,
        IUrlValidator urlValidator)
    {
        _logger = logger;
        _cacheManager = cacheManager;
        _urlValidator = urlValidator;
        
        // Try to get from cache first, fall back to config
        var cachedUrl = _cacheManager.GetStringAsync(CacheKeys.InferenceEngineUrl).Result;
        _currentBaseUrl = cachedUrl ?? configuration["InferenceEngine:BaseUrl"];
        
        // For backward compatibility, also check the old config path
        if (string.IsNullOrEmpty(_currentBaseUrl))
        {
            _currentBaseUrl = configuration["OllamaApi:BaseUrl"];
        }
        
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
        await _cacheManager.SetStringAsync(CacheKeys.InferenceEngineUrl, newUrl);
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
    public bool IsValid(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;
            
        try
        {
            var uri = new Uri(url);
            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
        }
        catch
        {
            return false;
        }
    }
}
```

### 6. InferenceEngineConnector

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

public class InferenceEngineConnector : IInferenceEngineConnector, IOllamaConnector
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
        _logger.LogInformation("Updating InferenceEngine client for new URL: {Url}", newUrl);
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

### 9. RabbitMQ Consumer

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

## Configuration Settings

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_DB_CONNECTION",
    "Redis": "YOUR_REDIS_CONNECTION"
  },
  "InferenceEngine": {
    "BaseUrl": "http://localhost:11434"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "Exchange": "service-discovery",
    "InferenceUrlQueue": "inference-url-updates",
    "InferenceUrlRoutingKey": "inference.url.changed"
  }
}
```

## Dependency Injection

```csharp
// Add these new extension methods to ServiceExtensions.cs

public static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
{
    services.AddSingleton<ICacheManager, RedisCacheService>();
    
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

// Update connector registration to support both interfaces during transition
public static IServiceCollection AddInferenceEngineConnector(this IServiceCollection services)
{
    // Register the new connector that implements both interfaces
    services.AddScoped<InferenceEngineConnector>();
    
    // Register interfaces to the same implementation for DI
    services.AddScoped<IInferenceEngineConnector>(sp => sp.GetRequiredService<InferenceEngineConnector>());
    services.AddScoped<IOllamaConnector>(sp => sp.GetRequiredService<InferenceEngineConnector>());
    
    return services;
}
```

## Program.cs Updates

```csharp
// Update in Program.cs or Startup.cs:

// Add Caching
services.AddCaching(Configuration);

// Add Messaging
services.AddMessaging(Configuration);

// Add InferenceEngineConnector (this replaces the old OllamaConnector registration)
services.AddInferenceEngineConnector();
```

## Health Monitoring

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

## Directory Structure

```
AdminService/
├── Infrastructure/
│   ├── Caching/
│   │   ├── Interfaces/
│   │   │   └── ICacheManager.cs
│   │   ├── RedisCacheService.cs
│   │   └── CacheKeys.cs
│   ├── Configuration/
│   │   ├── Options/
│   │   │   ├── InferenceEngineOptions.cs
│   │   │   └── RabbitMQOptions.cs
│   │   ├── IInferenceEngineConfiguration.cs
│   │   └── InferenceEngineConfiguration.cs
│   ├── Messaging/
│   │   ├── Consumers/
│   │   │   └── InferenceUrlConsumer.cs
│   │   ├── Models/
│   │   │   └── InferenceUrlUpdateMessage.cs
│   │   └── Resilience/
│   │       └── RabbitMQResiliencePolicies.cs
│   └── Validation/
│       ├── IUrlValidator.cs
│       └── UrlValidator.cs
├── Connectors/
│   ├── IInferenceEngineConnector.cs
│   ├── InferenceEngineConnector.cs
│   ├── IOllamaConnector.cs (existing)
│   └── OllamaConnector.cs (deprecated, will be removed after transition)
```

## NuGet Package Dependencies

- RabbitMQ.Client
- Polly
- Polly.Extensions.Http
- StackExchange.Redis
- System.Text.Json

## Implementation Steps

### Phase 1: Infrastructure Setup
1. Create the Infrastructure directory structure
2. Add required NuGet packages to the project
3. Implement ICacheManager and RedisCacheService
4. Create CacheKeys class
5. Implement configuration options classes (InferenceEngineOptions, RabbitMQOptions)

### Phase 2: Messaging Implementation
1. Implement InferenceUrlUpdateMessage
2. Implement RabbitMQResiliencePolicies
3. Implement InferenceUrlConsumer
4. Implement URL validators

### Phase 3: Connector Transition
1. Create IInferenceEngineConnector interface
2. Implement InferenceEngineConnector that implements both IInferenceEngineConnector and IOllamaConnector
3. Keep existing OllamaConnector temporarily
4. Update ServiceExtensions.cs to register both interfaces to the same implementation

### Phase 4: Service Integration
1. Update InferenceOperationsService to use IInferenceEngineConnector
2. Create HealthController
3. Update appsettings.json with new configuration sections
4. Update Program.cs or Startup.cs to use the new extensions

### Phase 5: Testing and Cleanup
1. Test the implementation with RabbitMQ
2. Test fallback to configuration
3. Test URL updates and connector behavior
4. Remove OllamaConnector.cs when transition is complete

## Testing Plan

1. Verify RabbitMQ connection with debug logging
2. Test URL update messages through the RabbitMQ queue
3. Verify the InferenceEngineConnector uses the updated URL for making API calls
4. Test fallback to appsettings.json when RabbitMQ is unavailable
5. Verify URL persistence in Redis across service restarts
6. Test health endpoint responses
7. Verify model operations with dynamically updated URLs
8. Verify backward compatibility during transition
9. Test resilience policies with simulated failures

## Migration Considerations

1. Both IInferenceEngineConnector and IOllamaConnector will be supported during the transition
2. Services can be gradually migrated to use IInferenceEngineConnector
3. InferenceEngineConnector implements both interfaces for seamless transition
4. Configuration paths are preserved with backward compatibility
5. Once all services have been migrated, OllamaConnector can be removed
