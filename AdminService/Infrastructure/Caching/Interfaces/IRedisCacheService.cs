using System;
using System.Threading.Tasks;

namespace AdminService.Infrastructure.Caching.Interfaces
{
    /// <summary>
    /// Interface for cache operations
    /// </summary>
    public interface  IRedisCacheService
    {
        /// <summary>
        /// Gets a value from the cache
        /// </summary>
        /// <param name="key">The cache key</param>
        /// <returns>The cached value or default if not found</returns>
        Task<T?> GetAsync<T>(string key);
        
        /// <summary>
        /// Sets a value in the cache
        /// </summary> 
        /// <param name="key">The cache key</param>
        /// <param name="value">The value to cache</param>
        /// <param name="expiration">Optional expiration timespan</param>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

        /// <summary>
        /// Removes a value from the cache
        /// </summary>
        /// <param name="key">The cache key to remove</param>
        Task RemoveAsync(string key);
        
        /// <summary>
        /// Checks if a key exists in the cache
        /// </summary>
        /// <param name="key">The cache key to check</param>
        /// <returns>True if the key exists, false otherwise</returns>
        Task<bool> ExistsAsync(string key);
        
        /// <summary>
        /// Checks if the Redis connection is available
        /// </summary>
        /// <returns>True if the connection is available, false otherwise</returns>
        Task<bool> IsConnectionAvailableAsync();
    }
} 