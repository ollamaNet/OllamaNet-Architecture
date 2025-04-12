using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OllamaSharp.Models;
using AdminService.DTOs;
using Model = OllamaSharp.Models.Model;
using AdminService.DTOs;


namespace AdminService.Connectors
{
    public interface IOllamaConnector
    {
        IReadOnlyDictionary<string, object?> Attributes { get; }
        Task<IEnumerable<Model>> GetInstalledModels();
        Task<IEnumerable<Model>> GetInstalledModelsPaged(int pageNumber, int PageSize);
        Task<ShowModelResponse> GetModelInfo(string modelName);
        Task<string> RemoveModel(string modelName);
        IAsyncEnumerable<InstallProgressInfo> PullModelAsync(string modelName);

    }
}