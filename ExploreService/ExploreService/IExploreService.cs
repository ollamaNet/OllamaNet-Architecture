using ExploreService.DTOs;
using Ollama_DB_layer.DataBaseHelpers;
using Ollama_DB_layer.DTOs;

namespace ExploreService
{
    public interface IExploreService
    {
        Task<PagedResult<ModelCard>> AvailableModels(int PageNumber, int PageSize);

        Task<ModelInfoResponse> ModelInfo(string modelID);

        Task<List<GetTagsResponse>> GetTags();
        
        Task<IEnumerable<ModelCard>> GetTagModels(string tagId);
    }
}