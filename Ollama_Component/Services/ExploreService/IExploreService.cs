using Ollama_Component.Services.ExploreService.Models;
using Ollama_DB_layer.Entities;
using Ollama_DB_layer.Helpers;

namespace Ollama_Component.Services.ExploreService
{
    public interface IExploreService
    {
        Task<GetPagedModelsResponse> AvailableModels(GetPagedModelsRequest request);
        Task<ModelInfoResponse> ModelInfo(string modelID);

    }
}