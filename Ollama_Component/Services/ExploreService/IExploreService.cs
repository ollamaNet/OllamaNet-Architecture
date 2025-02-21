using Ollama_Component.Services.ExploreService.Models;
namespace Ollama_Component.Services.ExploreService
{
    public interface IExploreService
    {
        Task<GetPagedModelsResponse> AvailableModels(GetPagedModelsRequest request);
        Task<ModelInfoResponse> ModelInfo(string modelID);

    }
}