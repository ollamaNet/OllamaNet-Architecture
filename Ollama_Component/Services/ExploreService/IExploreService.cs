using Ollama_Component.Services.ExploreService.Models;
using Ollama_DB_layer.DataBaseHelpers;
using Ollama_DB_layer.DTOs;
using Ollama_DB_layer.Entities;

namespace Ollama_Component.Services.ExploreService
{
    public interface IExploreService
    {
        Task<PagedResult<ModelCard>> AvailableModels(int PageNumber, int PageSize);

        Task<ModelInfoResponse> ModelInfo(string modelID);

        Task<List<GetTagsResponse>> GetTags();
    }
}