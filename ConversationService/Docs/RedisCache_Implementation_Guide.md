# Redis Caching Implementation Guide for Microservices

## Overview

This guide explains how to implement Redis caching with proper logging and exception handling in a microservice architecture. The approach described here follows the patterns established in the ExploreService, providing a consistent and robust caching strategy across services.

## Architecture Components

A complete caching implementation consists of these key components:

1. **Cache Settings** - Configuration for Redis and cache parameters
2. **Cache Keys** - Centralized cache key management
3. **Redis Cache Service** - Low-level Redis operations
4. **Cache Manager** - High-level caching abstraction with fallback strategies
5. **Exception Handling** - Specialized exceptions for cache operations
6. **Logging** - Comprehensive logging strategy

## Implementation Steps

### 1. Cache Settings

Create a settings class to hold Redis configuration:

```csharp
// RedisCacheSettings.cs
namespace YourService.Cache;

public class RedisCacheSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string InstanceName { get; set; } = string.Empty;
    public int DefaultExpirationMinutes { get; set; }
    
    // Service-specific expirations
    public int ResourceSpecificExpirationMinutes { get; set; }
    
    // Timeout settings
    public int OperationTimeoutMilliseconds { get; set; } = 1000;
    
    // Retry settings
    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryDelayMilliseconds { get; set; } = 100;
    public int RetryDelayMultiplier { get; set; } = 5;
}
```

Register in Program.cs or Startup.cs:

```csharp
services.Configure<RedisCacheSettings>(Configuration.GetSection("RedisCache"));
```

### 2. Cache Keys

Create a central location for cache key formats:

```csharp
// CacheKeys.cs
namespace YourService.Cache;

public static class CacheKeys
{
    public const string ResourceList = "resource:list:page:{0}:size:{1}";
    public const string ResourceInfo = "resource:info:{0}"; // {0} = resourceId
    // Add more key patterns as needed
}
```

### 3. Cache Exceptions

Create a hierarchy of cache-specific exceptions:

```csharp
// Cache/Exceptions/CacheExceptions.cs
namespace YourService.Cache.Exceptions;

/// <summary>
/// Base exception for all cache-related exceptions
/// </summary>
public class CacheException : Exception
{
    public string CacheKey { get; }

    public CacheException(string message) : base(message) { }
    public CacheException(string message, Exception innerException) : base(message, innerException) { }
    public CacheException(string message, string cacheKey) : base(message)
    {
        CacheKey = cacheKey;
    }
    public CacheException(string message, string cacheKey, Exception innerException) : base(message, innerException)
    {
        CacheKey = cacheKey;
    }
}

/// <summary>
/// Exception thrown when Redis connection fails
/// </summary>
public class CacheConnectionException : CacheException
{
    // Add constructors as needed
}

/// <summary>
/// Exception thrown when serialization/deserialization fails
/// </summary>
public class CacheSerializationException : CacheException
{
    // Add constructors as needed
}

// Add more exception types as needed
```

### 4. Redis Cache Service

Implement the low-level Redis operations:

```csharp
// RedisCacheService.cs
namespace YourService.Cache;

public interface IRedisCacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
    Task<bool> IsConnectionAvailableAsync();
}

public class RedisCacheService : IRedisCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly RedisCacheSettings _settings;

    public RedisCacheService(
        IConnectionMultiplexer redis, 
        IOptions<RedisCacheSettings> settings,
        ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _settings = settings.Value;
        _logger = logger;
    }

    // Implement the interface methods with proper logging and exception handling
    // See ExploreService.Cache.RedisCacheService for a complete implementation
}
```

### 5. Cache Manager

Implement the high-level caching abstraction:

```csharp
// CacheManager.cs
namespace YourService.Cache;

public interface ICacheManager
{
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> dataProvider, TimeSpan? expiration = null);
    Task InvalidateCache(string key);
    Task ClearCache();
}

public class CacheManager : ICacheManager
{
    private readonly IRedisCacheService _cacheService;
    private readonly ILogger<CacheManager> _logger;
    private readonly RedisCacheSettings _settings;

    public CacheManager(
        IRedisCacheService cacheService,
        IOptions<RedisCacheSettings> settings,
        ILogger<CacheManager> logger)
    {
        _cacheService = cacheService;
        _settings = settings.Value;
        _logger = logger;
    }

    // Implement the interface methods with proper retry logic and fallback
    // See ExploreService.Cache.CacheManager for a complete implementation
}
```

### 6. Service-Level Exceptions

Create service-specific exceptions that wrap cache exceptions:

```csharp
// Exceptions/ServiceExceptions.cs
namespace YourService.Exceptions;

// Base exception for all service exceptions
public class ServiceException : Exception
{
    // Add constructors as needed
}

// Exception for resource not found
public class ResourceNotFoundException : ServiceException
{
    // Add constructors and properties as needed
}

// Exception for data retrieval issues
public class DataRetrievalException : ServiceException
{
    public string ResourceType { get; }
    
    // Add constructors that include resource type
}

// Helper to convert cache exceptions to service exceptions
public static class ExceptionConverter
{
    public static ServiceException ConvertCacheException(CacheException cacheEx, string resourceType)
    {
        // Convert cache exceptions to service-level exceptions
    }
}
```

### 7. Using Caching in Your Service

Implement caching in your service:

```csharp
// YourService.cs
public class YourService : IYourService
{
    private readonly ICacheManager _cacheManager;
    private readonly ILogger<YourService> _logger;
    private readonly RedisCacheSettings _settings;
    private readonly IDataRepository _repository;

    // Constructor with DI

    public async Task<ResourceResponse> GetResourceById(string id)
    {
        _logger.LogInformation("Retrieving resource with ID: {ResourceId}", id);
        var cacheKey = string.Format(CacheKeys.ResourceInfo, id);
        var expiration = TimeSpan.FromMinutes(_settings.ResourceSpecificExpirationMinutes);

        try
        {
            return await _cacheManager.GetOrSetAsync(
                cacheKey,
                async () => {
                    _logger.LogDebug("Fetching resource from database. ID: {ResourceId}", id);
                    var resource = await _repository.GetByIdAsync(id);
                    
                    if (resource == null)
                    {
                        throw new ResourceNotFoundException(id);
                    }
                    
                    return MapToResponse(resource);
                },
                expiration
            );
        }
        catch (CacheException cacheEx)
        {
            throw ExceptionConverter.ConvertCacheException(cacheEx, $"resource '{id}'");
        }
        catch (Exception ex) when (ex is not ResourceNotFoundException && ex is not ServiceException)
        {
            _logger.LogError(ex, "Error retrieving resource with ID: {ResourceId}", id);
            throw new DataRetrievalException($"resource '{id}'", ex);
        }
    }
}
```

## Logging Strategy

Implement the following logging approach:

### Service Layer
- **Information level**: Log operation start/completion with business context
- **Error level**: Log exceptions with context
- **Debug level**: Log database operations

### Cache Manager
- **Information level**: Log cache hits/misses
- **Warning level**: Log fallbacks to data source
- **Debug level**: Log cache operations details

### Redis Cache Service
- **Debug level**: Log Redis operations with timing
- **Warning level**: Log connection issues
- **Error level**: Log Redis operation failures

## Configuration in appsettings.json

```json
{
  "RedisCache": {
    "ConnectionString": "your-redis-connection-string",
    "InstanceName": "your-service-name:",
    "DefaultExpirationMinutes": 30,
    "ResourceSpecificExpirationMinutes": 60,
    "OperationTimeoutMilliseconds": 1000,
    "MaxRetryAttempts": 3,
    "RetryDelayMilliseconds": 100,
    "RetryDelayMultiplier": 5
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "YourService.Cache": "Debug"
    }
  }
}
```

## Registration in DI Container

```csharp
// Program.cs or Startup.cs
services.Configure<RedisCacheSettings>(Configuration.GetSection("RedisCache"));

// Register Redis connection
services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<RedisCacheSettings>>().Value;
    return ConnectionMultiplexer.Connect(settings.ConnectionString);
});

// Register cache services
services.AddSingleton<IRedisCacheService, RedisCacheService>();
services.AddSingleton<ICacheManager, CacheManager>();
```

## Best Practices

1. **Cache Key Conventions**:
   - Use namespaced keys (`service:entity:id`)
   - Add pagination info for lists (`service:entity:list:page:1:size:20`)
   - Include relevant filters in keys

2. **Cache Duration Strategy**:
   - Consider data volatility - how often does it change?
   - Use shorter durations for frequently changing data
   - Use longer durations for reference data
   - Configure durations in settings, not hardcoded

3. **Error Handling**:
   - Always fall back to data source when cache fails
   - Implement retry logic for transient failures
   - Use specific exception types for better debugging

4. **Cache Invalidation**:
   - Implement event-based invalidation when data changes
   - Consider using pub/sub for cross-service invalidation
   - Set reasonable TTLs as a safety mechanism

## Advanced Features for Future Implementation

1. **Circuit Breaker Pattern**:
   - Temporarily disable cache when Redis is failing
   - Auto-recover when Redis becomes available again

2. **Tiered Caching**:
   - Add in-memory cache as L1 cache (faster but not shared)
   - Use Redis as L2 cache (shared across instances)

3. **Distributed Locking**:
   - Implement Redis-based locks for concurrent operations
   - Use for "update if not changed" scenarios

4. **Batch Operations**:
   - Implement bulk get/set operations for performance
   - Use Redis transactions where appropriate

By following this guide, you'll implement a robust, consistent caching strategy across your microservices while maintaining proper separation of concerns and error handling. 