using AdminService.Services.AIModelOperations.DTOs;

namespace AdminService.Services.AIModelOperations
{
    public interface IAIModelOperationsService
    {
        // Get/Read operations
        Task<AIModelResponse> GetModelByIdAsync(string modelId);
        
        // Create operations
        Task<ModelOperationResult> CreateModelAsync(CreateModelRequest request, string userId);
        
        // Update operations
        Task<ModelOperationResult> UpdateModelAsync(UpdateModelRequest request);
        Task<ModelOperationResult> AddTagsToModelAsync(ModelTagOperationRequest request);
        Task<ModelOperationResult> RemoveTagsFromModelAsync(ModelTagOperationRequest request);
        
        // Delete operations
        Task<ModelOperationResult> DeleteModelAsync(string modelId);
        Task<ModelOperationResult> SoftDeleteModelAsync(string modelId);
    }
} 