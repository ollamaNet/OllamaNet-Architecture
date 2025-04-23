using System;
using ExploreService.Cache.Exceptions;

namespace ExploreService.Exceptions
{
    /// <summary>
    /// Base exception for all ExploreService exceptions
    /// </summary>
    public class ExploreServiceException : Exception
    {
        public ExploreServiceException(string message) : base(message) { }
        public ExploreServiceException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when a model is not found
    /// </summary>
    public class ModelNotFoundException : ExploreServiceException
    {
        public string ModelId { get; }
        
        public ModelNotFoundException(string modelId) 
            : base($"Model with ID '{modelId}' was not found")
        {
            ModelId = modelId;
        }
    }

    /// <summary>
    /// Exception thrown when a tag is not found
    /// </summary>
    public class TagNotFoundException : ExploreServiceException
    {
        public string TagId { get; }
        
        public TagNotFoundException(string tagId)
            : base($"Tag with ID '{tagId}' was not found")
        {
            TagId = tagId;
        }
    }

    /// <summary>
    /// Exception thrown when there are data retrieval issues
    /// </summary>
    public class DataRetrievalException : ExploreServiceException
    {
        public string ResourceType { get; }
        
        public DataRetrievalException(string resourceType, string message) 
            : base($"Failed to retrieve {resourceType}: {message}")
        {
            ResourceType = resourceType;
        }
        
        public DataRetrievalException(string resourceType, Exception innerException) 
            : base($"Failed to retrieve {resourceType}", innerException)
        {
            ResourceType = resourceType;
        }
    }

    /// <summary>
    /// Helper class to convert cache exceptions to service exceptions
    /// </summary>
    public static class ExceptionConverter
    {
        /// <summary>
        /// Converts a cache exception to an appropriate service exception
        /// </summary>
        public static ExploreServiceException ConvertCacheException(CacheException cacheEx, string resourceType)
        {
            return new DataRetrievalException(resourceType, 
                $"Cache error: {GetCacheErrorDescription(cacheEx)}");
        }

        private static string GetCacheErrorDescription(CacheException cacheEx)
        {
            return cacheEx switch
            {
                CacheConnectionException => "Connection to cache failed",
                CacheTimeoutException timeoutEx => $"Operation timed out after {timeoutEx.Threshold.TotalMilliseconds}ms",
                CacheSerializationException => "Data format error",
                _ => cacheEx.Message
            };
        }
    }
} 