
using Ollama_Component.Services.AdminServices.Models;
using Ollama_DB_layer.Entities;
using OllamaSharp.Models;
using Model = OllamaSharp.Models.Model;

namespace Ollama_Component.Services.AdminServices
{
    public interface IAdminService
    {
        Task<IEnumerable<Model>> InstalledModelsAsync();
        Task<ShowModelResponse> ModelInfoAsync(string modelName);
        Task<AIModel> AddModelAsync(AddModelRequest model);
        Task<string> UninstllModelAsync(RemoveModelRequest model);
        Task<string> SoftDeleteAIModelAsync(string modelName);
        Task<InstallProgressInfo> InstallModelAsync(string modelName, IProgress<InstallProgressInfo>? progress = null);


    }
}