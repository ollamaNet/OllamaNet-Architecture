namespace AdminService.Services.AIModelOperations.DTOs
{
    public class ModelOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ModelId { get; set; }
        
        public static ModelOperationResult CreateSuccess(string modelId, string message = "Operation completed successfully")
        {
            return new ModelOperationResult
            {
                Success = true,
                Message = message,
                ModelId = modelId
            };
        }
        
        public static ModelOperationResult CreateFailure(string message)
        {
            return new ModelOperationResult
            {
                Success = false,
                Message = message
            };
        }
    }
} 