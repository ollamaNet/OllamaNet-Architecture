using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ExploreService.Cache;

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

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> dataProvider, TimeSpan? expiration = null)
    {
        try
        {
            // Try to get from cache first
            var cachedValue = await _cacheService.GetAsync<T>(key);
            if (cachedValue != null)
            {
                _logger.LogInformation("Cache hit for key: {Key}", key);
                return cachedValue;
            }

            _logger.LogInformation("Cache miss for key: {Key}, fetching from data provider", key);

            // If not in cache, get from data provider
            var value = await dataProvider();

            // Store in cache with expiration
            var expirationTime = expiration ?? TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes);
            await _cacheService.SetAsync(key, value, expirationTime);

            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetOrSetAsync for key: {Key}", key);
            
            // If cache operations fail, fall back to data provider
            _logger.LogWarning("Falling back to data provider for key: {Key}", key);
            return await dataProvider();
        }
    }

    public async Task InvalidateCache(string key)
    {
        try
        {
            await _cacheService.RemoveAsync(key);
            _logger.LogInformation("Successfully invalidated cache for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for key: {Key}", key);
            throw;
        }
    }

    public async Task ClearCache()
    {
        try
        {
            // Note: This is a simplified implementation. In a production environment,
            // you might want to implement a more sophisticated cache clearing strategy
            // based on your specific needs and Redis configuration.
            _logger.LogWarning("Clearing entire cache - this operation should be used with caution");
            // Implementation depends on your Redis setup and requirements
            throw new NotImplementedException("Cache clearing not implemented - implement based on your specific requirements");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
            throw;
        }
    }
} 