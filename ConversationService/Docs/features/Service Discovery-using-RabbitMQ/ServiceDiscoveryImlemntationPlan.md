# InferenceEngine Service Discovery Implementation Plan

This document outlines the detailed implementation plan for the RabbitMQ-based service discovery solution for dynamically updating the InferenceEngine URL. The plan is divided into phases with specific actions, files, and code for each step.

## Phase 1: Project Setup and Required Libraries

### 1.1 Create Directory Structure

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
```

### 1.2 Add Required NuGet Packages

Add the following NuGet packages to the project:

```xml
<ItemGroup>
  <PackageReference Include="RabbitMQ.Client" Version="6.5.0" />
  <PackageReference Include="Polly" Version="7.2.4" />
  <PackageReference Include="Polly.Extensions.Http" Version="3.0.0" />
  <PackageReference Include="Microsoft.Extensions.Http.Polly" Version="7.0.10" />
</ItemGroup>
```

### 1.3 Update appsettings.json Configuration

Add the following sections to appsettings.json:

```json
{
  "InferenceEngine": {
    "BaseUrl": "https://default-inference-engine-url.ngrok-free.app/"
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

## Phase 2: Core Configuration and Validation Components

### 2.1 Create URL Validator

**File:** `Infrastructure/Messaging/Validators/UrlValidator.cs`

```csharp
using System.Text.RegularExpressions;

namespace ConversationService.Infrastructure.Messaging.Validators;

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

**Responsibility:** Validates that incoming URLs match the required format for ngrok URLs.

### 2.2 Create RabbitMQ Options

**File:** `Infrastructure/Messaging/Options/RabbitMQOptions.cs`

```csharp
namespace ConversationService.Infrastructure.Messaging.Options;

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

**Responsibility:** Contains configuration properties for connecting to RabbitMQ.

### 2.3 Create Message Model

**File:** `Infrastructure/Messaging/Models/InferenceUrlUpdateMessage.cs`

```csharp
using System;

namespace ConversationService.Infrastructure.Messaging.Models;

public class InferenceUrlUpdateMessage
{
    public string NewUrl { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string ServiceId { get; set; } = "inference-engine";
    public string Version { get; set; } = "1.0";
}
```

**Responsibility:** Defines the structure of messages exchanged via RabbitMQ for URL updates.

### 2.4 Create InferenceEngine Configuration Service

**File:** `Infrastructure/Configuration/InferenceEngineConfiguration.cs`

```csharp
using System;
using System.Threading.Tasks;
using ConversationService.Infrastructure.Caching;
using ConversationService.Infrastructure.Messaging.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConversationService.Infrastructure.Configuration;

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

**Responsibility:** Manages the current InferenceEngine URL, handles persistence via Redis, and notifies subscribers when the URL changes.

## Phase 3: Resilience and Messaging Infrastructure

### 3.1 Create Resilience Policies

**File:** `Infrastructure/Messaging/Resilience/RabbitMQResiliencePolicies.cs`

```csharp
using System;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;

namespace ConversationService.Infrastructure.Messaging.Resilience;

public class RabbitMQResiliencePolicies
{
    public IAsyncPolicy RetryPolicy { get; }
    public ICircuitBreakerPolicy CircuitBreakerPolicy { get; }
    public bool IsConnected => CircuitBreakerPolicy.CircuitState != CircuitState.Open;

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

**Responsibility:** Provides Polly-based resilience policies for RabbitMQ connections, including retry and circuit breaker patterns.

### 3.2 Create Message Consumer Interface

**File:** `Infrastructure/Messaging/Interfaces/IMessageConsumer.cs`

```csharp
using System.Threading;
using System.Threading.Tasks;

namespace ConversationService.Infrastructure.Messaging.Interfaces;

public interface IMessageConsumer
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
    bool IsConnected { get; }
}
```

**Responsibility:** Defines the contract for message consumers, including connection status.

### 3.3 Create RabbitMQ Consumer

**File:** `Infrastructure/Messaging/Consumers/InferenceUrlConsumer.cs`

```csharp
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
using Polly.CircuitBreaker;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ConversationService.Infrastructure.Messaging.Consumers;

public class InferenceUrlConsumer : BackgroundService, IMessageConsumer
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

**Responsibility:** Connects to RabbitMQ, listens for URL update messages, and forwards them to the configuration service.

## Phase 4: Dependency Injection and Service Extensions

### 4.1 Create Messaging Extensions

**File:** `Infrastructure/Messaging/Extensions/MessagingServiceExtensions.cs`

```csharp
using ConversationService.Infrastructure.Configuration;
using ConversationService.Infrastructure.Messaging.Consumers;
using ConversationService.Infrastructure.Messaging.Options;
using ConversationService.Infrastructure.Messaging.Resilience;
using ConversationService.Infrastructure.Messaging.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConversationService.Infrastructure.Messaging.Extensions;

public static class MessagingServiceExtensions
{
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
}
```

**Responsibility:** Provides extension methods to register messaging services in the dependency injection container.

## Phase 5: Rename and Update InferenceEngine Connector

### 5.1 Create InferenceEngine Interface

**File:** `Infrastructure/Integration/InferenceEngine/IInferenceEngineConnector.cs`

```csharp
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ConversationService.Services.Chat.DTOs;

namespace ConversationService.Infrastructure.Integration.InferenceEngine;

public interface IInferenceEngineConnector
{
    Task<OllamaModelResponse> GetChatMessageContentsAsync(PromptRequest request, CancellationToken cancellationToken = default);
    IAsyncEnumerable<OllamaModelResponse> GetStreamedChatMessageContentsAsync(PromptRequest request, CancellationToken cancellationToken = default);
    Task<string[]> GetAvailableModelsAsync(CancellationToken cancellationToken = default);
    string GetBaseUrl();
}
```

**Responsibility:** Defines the contract for the InferenceEngine connector, replacing the IOllamaConnector interface.

### 5.2 Create InferenceEngine Connector Implementation

**File:** `Infrastructure/Integration/InferenceEngine/InferenceEngineConnector.cs`

```csharp
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ConversationService.Infrastructure.Configuration;
using ConversationService.Services.Chat.DTOs;
using Microsoft.Extensions.Logging;

namespace ConversationService.Infrastructure.Integration.InferenceEngine;

public class InferenceEngineConnector : IInferenceEngineConnector
{
    private readonly IInferenceEngineConfiguration _configuration;
    private readonly ILogger<InferenceEngineConnector> _logger;
    private HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public InferenceEngineConnector(
        IInferenceEngineConfiguration configuration,
        ILogger<InferenceEngineConnector> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
        
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
            BaseAddress = new Uri(_configuration.GetBaseUrl())
        };
        
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        return client;
    }

    public async Task<OllamaModelResponse> GetChatMessageContentsAsync(PromptRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending chat request to inference engine at {BaseUrl}", _httpClient.BaseAddress);
            
            var response = await _httpClient.PostAsJsonAsync("/api/chat", request, _jsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<OllamaModelResponse>(_jsonOptions, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat message from inference engine");
            throw;
        }
    }

    public async IAsyncEnumerable<OllamaModelResponse> GetStreamedChatMessageContentsAsync(
        PromptRequest request, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending streaming chat request to inference engine at {BaseUrl}", _httpClient.BaseAddress);
            
            var content = new StringContent(
                JsonSerializer.Serialize(request, _jsonOptions),
                Encoding.UTF8,
                "application/json");
                
            var response = await _httpClient.PostAsync("/api/chat/stream", content, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new System.IO.StreamReader(stream);
            
            string line;
            while ((line = await reader.ReadLineAsync()) != null && !cancellationToken.IsCancellationRequested)
            {
                if (string.IsNullOrEmpty(line) || line.StartsWith(":"))
                    continue;
                    
                if (line.StartsWith("data:"))
                    line = line.Substring(5);
                    
                var chunk = JsonSerializer.Deserialize<OllamaModelResponse>(line, _jsonOptions);
                if (chunk != null)
                    yield return chunk;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting streamed chat message from inference engine");
            throw;
        }
    }

    public async Task<string[]> GetAvailableModelsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting available models from inference engine at {BaseUrl}", _httpClient.BaseAddress);
            
            var response = await _httpClient.GetAsync("/api/models", cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var models = await response.Content.ReadFromJsonAsync<string[]>(_jsonOptions, cancellationToken);
            return models ?? Array.Empty<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available models from inference engine");
            return Array.Empty<string>();
        }
    }

    public string GetBaseUrl() => _configuration.GetBaseUrl();
}
```

**Responsibility:** Implements the InferenceEngine connector interface, replacing the OllamaConnector class.

## Phase 6: Update Service Registration and Program.cs

### 6.1 Update ServiceExtensions.cs

**File:** `ServiceExtensions.cs`

```csharp
// Add these new using statements
using ConversationService.Infrastructure.Configuration;
using ConversationService.Infrastructure.Integration.InferenceEngine;
using ConversationService.Infrastructure.Messaging.Extensions;

namespace ConversationService;

public static class ServiceExtensions
{
    // Existing methods...
    
    // Replace the existing AddOllamaConnector method with this
    public static IServiceCollection AddInferenceEngine(this IServiceCollection services)
    {
        services.AddSingleton<IInferenceEngineConnector, InferenceEngineConnector>();
        return services;
    }
    
    // Add this new method
    public static IServiceCollection AddServiceDiscovery(this IServiceCollection services, IConfiguration configuration)
    {
        // Add messaging services
        services.AddMessaging(configuration);
        
        return services;
    }
}
```

### 6.2 Update Program.cs

**File:** `Program.cs`

```csharp
// Find where services.AddSingleton<IOllamaConnector, OllamaConnector>() is registered
// and replace it with:
services.AddInferenceEngine();
services.AddServiceDiscovery(Configuration);
```

## Phase 7: Update References in Dependent Services

The following files need to be updated to use IInferenceEngineConnector instead of IOllamaConnector:

### 7.1 Update ChatService.cs

**File:** `Services/Chat/ChatService.cs`

```csharp
// Change from:
private readonly IOllamaConnector _ollamaConnector;

// To:
private readonly IInferenceEngineConnector _inferenceEngineConnector;

// Update constructor and any usages
```

### 7.2 Update Other References

Search for all occurrences of `OllamaConnector` and `IOllamaConnector` in the codebase and replace them with `InferenceEngineConnector` and `IInferenceEngineConnector` respectively.

Files likely to be affected:
- `Services/Chat/ChatService.cs`
- `Controllers/ChatController.cs`
- Any test files that use the OllamaConnector

## Files Affected by Renaming Operation

Based on the ConversationService memory bank and SystemDesign.md, the following files will likely be affected by renaming OllamaConnector to InferenceEngineConnector:

1. `Infrastructure/Integration/OllamaConnector.cs` → `Infrastructure/Integration/InferenceEngine/InferenceEngineConnector.cs`
2. `Infrastructure/Integration/IOllamaConnector.cs` → `Infrastructure/Integration/InferenceEngine/IInferenceEngineConnector.cs`
3. `Services/Chat/ChatService.cs` (uses OllamaConnector)
4. `ServiceExtensions.cs` (registers OllamaConnector)
5. `Program.cs` (configures OllamaConnector)
6. `Controllers/ChatController.cs` (may use OllamaConnector directly)
7. `appsettings.json` (contains OllamaApi section)
8. Any test files that mock or use OllamaConnector

## Complete Configuration for appsettings.json

```json
{
  "InferenceEngine": {
    "BaseUrl": "https://default-inference-engine-url.ngrok-free.app/"
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

## Testing Plan

After implementation, the following tests should be performed:

1. Verify that the InferenceUrlConsumer successfully connects to RabbitMQ
2. Test that URL updates received via RabbitMQ are properly applied
3. Verify that the InferenceEngineConnector uses the updated URL
4. Test fallback to appsettings.json when RabbitMQ is unavailable
5. Verify URL persistence in Redis across service restarts
6. Test URL validation with valid and invalid formats
