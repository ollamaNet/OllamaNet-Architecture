using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ConversationService.Cache.Exceptions;
using System.Diagnostics;

namespace ConversationService.Cache;

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
        var stopwatch = Stopwatch.StartNew();
        try
        {
            // Try to get from cache first
            _logger.LogDebug("Attempting to get value from cache for key: {Key}", key);
            var cachedValue = await _cacheService.GetAsync<T>(key);
            if (cachedValue != null)
            {
                stopwatch.Stop();
                _logger.LogInformation("Cache hit for key: {Key}. Retrieved in {ElapsedMilliseconds}ms", 
                    key, stopwatch.ElapsedMilliseconds);
                return cachedValue;
            }

            _logger.LogInformation("Cache miss for key: {Key}, fetching from data provider", key);

            // If not in cache, get from data provider
            var dataStopwatch = Stopwatch.StartNew();
            var value = await dataProvider();
            dataStopwatch.Stop();
            _logger.LogDebug("Data provider returned value in {ElapsedMilliseconds}ms", dataStopwatch.ElapsedMilliseconds);

            // Store in cache with expiration
            var expirationTime = expiration ?? TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes);
            try
            {
                await _cacheService.SetAsync(key, value, expirationTime);
                _logger.LogDebug("Successfully cached value for key: {Key} with expiration: {Expiration}", 
                    key, expirationTime);
            }
            catch (CacheException ex)
            {
                _logger.LogWarning(ex, "Failed to cache value for key: {Key}. Using data provider result directly.", key);
                // Continue execution - just don't cache the result
            }

            stopwatch.Stop();
            _logger.LogInformation("GetOrSetAsync operation completed in {ElapsedMilliseconds}ms for key: {Key}", 
                stopwatch.ElapsedMilliseconds, key);
            
            return value;
        }
        catch (CacheConnectionException ex)
        {
            _logger.LogWarning(ex, "Cache connection error for key: {Key}. Falling back to data provider.", key);
            return await ExecuteWithRetry(dataProvider, key, 0);
        }
        catch (CacheTimeoutException ex)
        {
            _logger.LogWarning(ex, "Cache timeout for key: {Key} after {Threshold}ms. Falling back to data provider.", 
                key, ex.Threshold.TotalMilliseconds);
            return await ExecuteWithRetry(dataProvider, key, 0);
        }
        catch (CacheSerializationException ex)
        {
            _logger.LogWarning(ex, "Cache serialization error for key: {Key}. Falling back to data provider.", key);
            return await ExecuteWithRetry(dataProvider, key, 0);
        }
        catch (CacheException ex)
        {
            _logger.LogWarning(ex, "Cache operation error for key: {Key}. Falling back to data provider.", key);
            return await ExecuteWithRetry(dataProvider, key, 0);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error in GetOrSetAsync for key: {Key} after {ElapsedMilliseconds}ms", 
                key, stopwatch.ElapsedMilliseconds);
            
            // If all else fails, fall back to data provider
            _logger.LogWarning("Falling back to data provider for key: {Key} due to unexpected error", key);
            return await dataProvider();
        }
    }



    private async Task<T> ExecuteWithRetry<T>(Func<Task<T>> operation, string key, int currentRetry)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex)
        {
            if (currentRetry >= _settings.MaxRetryAttempts - 1)
            {
                _logger.LogError(ex, "Operation failed after {RetryCount} retries for key: {Key}", 
                    _settings.MaxRetryAttempts, key);
                throw;
            }

            int delay = _settings.RetryDelayMilliseconds * (int)Math.Pow(_settings.RetryDelayMultiplier, currentRetry);
            _logger.LogWarning(ex, "Operation failed for key: {Key}. Retrying in {Delay}ms (attempt {RetryCount}/{MaxRetries})", 
                key, delay, currentRetry + 1, _settings.MaxRetryAttempts);

            await Task.Delay(delay);
            return await ExecuteWithRetry(operation, key, currentRetry + 1);
        }
    }

    public async Task InvalidateCache(string key)
    {
        try
        {
            _logger.LogInformation("Invalidating cache for key: {Key}", key);
            await _cacheService.RemoveAsync(key);
            _logger.LogInformation("Successfully invalidated cache for key: {Key}", key);
        }
        catch (CacheConnectionException ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate cache due to connection error for key: {Key}", key);
            // Swallow exception - the cache will expire naturally
        }
        catch (CacheException ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate cache for key: {Key}", key);
            // Swallow exception - the cache will expire naturally
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error invalidating cache for key: {Key}", key);
            // Rethrow for unexpected errors
            throw;
        }
    }

    public async Task ClearCache()
    {
        try
        {
            _logger.LogWarning("Clearing entire cache - this operation should be used with caution");
            
            // Note: This is a simplified implementation. 
            // In a production environment, you would implement a more sophisticated strategy
            // based on your specific Redis setup.
            throw new NotImplementedException("Cache clearing not implemented - implement based on your specific requirements");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
            throw;
        }
    }
} 