
using OllamaSharp.Models;

namespace Ollama_Component.Services
{
    public interface IAdminService
    {
        Task<IEnumerable<Model>> InstalledModelsAsync();
        Task<ShowModelResponse> GetModelInfo(string modelName);
    }
}