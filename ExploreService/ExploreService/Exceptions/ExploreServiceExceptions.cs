using System;

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
    /// Exception thrown when a cache operation fails
    /// </summary>
    public class CacheOperationException : ExploreServiceException
    {
        public string CacheKey { get; }
        
        public CacheOperationException(string cacheKey, Exception innerException) 
            : base($"Cache operation failed for key '{cacheKey}'", innerException)
        {
            CacheKey = cacheKey;
        }
    }
} 