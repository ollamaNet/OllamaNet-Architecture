using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ExploreService.Cache;

public interface IRedisCacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
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

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var db = _redis.GetDatabase();
            var value = await db.StringGetAsync(key);
            
            if (!value.HasValue)
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return default;
            }

            _logger.LogDebug("Cache hit for key: {Key}", key);
            return JsonSerializer.Deserialize<T>(value!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value from cache for key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var db = _redis.GetDatabase();
            var serializedValue = JsonSerializer.Serialize(value);
            
            await db.StringSetAsync(key, serializedValue, expiration);
            _logger.LogDebug("Successfully cached value for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in cache for key: {Key}", key);
            throw;
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(key);
            _logger.LogDebug("Successfully removed cache for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing value from cache for key: {Key}", key);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            var db = _redis.GetDatabase();
            return await db.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of key: {Key}", key);
            return false;
        }
    }
} 