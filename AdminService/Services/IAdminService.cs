//using AdminService.DTOs;
//using Ollama_DB_layer.Entities;

//namespace AdminService
//{
//    public interface IAdminService
//    {
//        Task<AIModel?> AddModelAsync(AddModelRequest model, string userId);
//        Task<string> SoftDeleteAIModelAsync(string modelName);
//        Task<List<Tag>> AddTags(List<string> tags);
//        Task<string> AddTagsToModel(string modelId, ICollection<AddTagToModelRequest> tags);
//        Task<IEnumerable<ApplicationUser>> GetAllUsers();
//        Task<string> UpdateModel(UpdateModelRequest model);
//    }
//}