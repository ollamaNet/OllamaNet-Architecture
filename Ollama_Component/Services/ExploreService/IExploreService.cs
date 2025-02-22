using Ollama_Component.Services.ExploreService.Models;
using Ollama_DB_layer.Entities;

namespace Ollama_Component.Services.ExploreService
{
    public interface IExploreService
    {
        Task<ModelCardsPaged> AvailableModels(int PageNumber, int PageSize);
        Task<ModelInfoResponse> ModelInfo(string modelID);

    }
}