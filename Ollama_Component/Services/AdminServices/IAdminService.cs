
using Ollama_Component.Services.AdminServices.Models;
using Ollama_DB_layer.Entities;
using OllamaSharp.Models;
using Model = OllamaSharp.Models.Model;

namespace Ollama_Component.Services
{
    public interface IAdminService
    {
        Task<IEnumerable<Model>> InstalledModelsAsync();
        Task<ShowModelResponse> GetModelInfo(string modelName);

        Task<AIModel> AddModelAsync(AddModelRequest model);

    }
}