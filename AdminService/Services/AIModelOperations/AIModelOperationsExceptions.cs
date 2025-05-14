using System;

namespace AdminService.Services.AIModelOperations.Exceptions
{
    /// <summary>
    /// Base exception for all AdminService AIModelOperations exceptions
    /// </summary>
    public class AIModelOperationsExceptions : Exception
    {
        public AIModelOperationsExceptions(string message) : base(message) { }
        public AIModelOperationsExceptions(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when a model is not found
    /// </summary>
    public class ModelNotFoundException : AIModelOperationsExceptions
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
    public class TagNotFoundException : AIModelOperationsExceptions
    {
        public Guid TagId { get; }
        
        public TagNotFoundException(Guid tagId)
            : base($"Tag with ID '{tagId}' was not found")
        {
            TagId = tagId;
        }
    }

    /// <summary>
    /// Exception thrown when there are data retrieval issues
    /// </summary>
    public class DataRetrievalException : AIModelOperationsExceptions
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
    /// Exception thrown when a model with the same name already exists
    /// </summary>
    public class ModelAlreadyExistsException : AIModelOperationsExceptions
    {
        public string ModelName { get; }
        public ModelAlreadyExistsException(string modelName)
            : base($"Model with name '{modelName}' already exists")
        {
            ModelName = modelName;
        }
    }

    /// <summary>
    /// Exception thrown when fetching model info from an external API fails
    /// </summary>
    public class ExternalModelFetchException : AIModelOperationsExceptions
    {
        public string ModelName { get; }
        public string ExternalError { get; }
        public ExternalModelFetchException(string modelName, string externalError)
            : base($"Failed to fetch model '{modelName}' from external API: {externalError}")
        {
            ModelName = modelName;
            ExternalError = externalError;
        }
    }

    // If you have cache exceptions, you can add a converter here, similar to ExploreService
    // public static class ExceptionConverter { ... }
} 