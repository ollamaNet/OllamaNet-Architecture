using System;

namespace ConversationServices.Infrastructure.Caching
{
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
        public CacheConnectionException(string message) : base(message) { }
        public CacheConnectionException(string message, Exception innerException) : base(message, innerException) { }
        public CacheConnectionException(string message, string cacheKey, Exception innerException) 
            : base(message, cacheKey, innerException) { }
    }

    /// <summary>
    /// Exception thrown when serialization/deserialization fails
    /// </summary>
    public class CacheSerializationException : CacheException
    {
        public CacheSerializationException(string message, string cacheKey) 
            : base(message, cacheKey) { }
        public CacheSerializationException(string message, string cacheKey, Exception innerException) 
            : base(message, cacheKey, innerException) { }
    }

    /// <summary>
    /// Exception thrown when a cache operation times out
    /// </summary>
    public class CacheTimeoutException : CacheException
    {
        public TimeSpan Threshold { get; }
        
        public CacheTimeoutException(string message, string cacheKey, TimeSpan threshold) 
            : base(message, cacheKey)
        {
            Threshold = threshold;
        }
        public CacheTimeoutException(string message, string cacheKey, TimeSpan threshold, Exception innerException) 
            : base(message, cacheKey, innerException)
        {
            Threshold = threshold;
        }
    }

    /// <summary>
    /// Exception thrown when a key is not found in cache
    /// </summary>
    public class CacheKeyNotFoundException : CacheException
    {
        public CacheKeyNotFoundException(string cacheKey) 
            : base($"Cache key '{cacheKey}' not found", cacheKey) { }
    }

    /// <summary>
    /// Exception thrown for general cache operations
    /// </summary>
    public class CacheOperationException : CacheException
    {
        public string Operation { get; }
        
        public CacheOperationException(string operation, string cacheKey) 
            : base($"Cache operation '{operation}' failed for key '{cacheKey}'", cacheKey)
        {
            Operation = operation;
        }
        
        public CacheOperationException(string operation, string cacheKey, Exception innerException) 
            : base($"Cache operation '{operation}' failed for key '{cacheKey}'", cacheKey, innerException)
        {
            Operation = operation;
        }
    }
} 