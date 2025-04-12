
using AdminService.DTOs;
using Ollama_DB_layer.Entities;
using OllamaSharp.Models;
using Model = OllamaSharp.Models.Model;

namespace AdminService
{
    public interface IAdminService
    {
        Task<IEnumerable<Model>> InstalledModelsAsync(int pageNumber, int PageSize);
        Task<ShowModelResponse> ModelInfoAsync(string modelName);
        Task<AIModel?> AddModelAsync(AddModelRequest model, string userId);

        Task<string> UninstllModelAsync(RemoveModelRequest model);
        Task<string> SoftDeleteAIModelAsync(string modelName);
        Task<InstallProgressInfo> InstallModelAsync(string modelName, IProgress<InstallProgressInfo>? progress = null);
        Task<List<Tag>> AddTags(List<string> tags);

        Task<string> AddTagsToModel(string modelId, ICollection<AddTagToModelRequest> tags);

        Task<IEnumerable<ApplicationUser>> GetAllUsers();

        Task<string> UpdateModel(UpdateModelRequest model);

    }
}