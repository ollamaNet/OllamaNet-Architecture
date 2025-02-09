using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OllamaSharp.Models;
using Ollama_Component.Services.ChatService.Models;
using Ollama_Component.Services.AdminServices.Models;
using Model = OllamaSharp.Models.Model;


namespace Ollama_Component.Connectors
{
    public interface IOllamaConnector
    {
        IReadOnlyDictionary<string, object?> Attributes { get; }
        Task<IReadOnlyList<ModelResponse>> GetChatMessageContentsAsync(ChatHistory chatHistory, PromptRequest request, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default);
        IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(ChatHistory chatHistory, PromptRequest request, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<Model>> GetInstalledModels();
        Task<ShowModelResponse> GetModelInfo(string modelName);
        Task<string> RemoveModel(string modelName);
        IAsyncEnumerable<InstallProgressInfo> PullModelAsync(string modelName);

    }
}