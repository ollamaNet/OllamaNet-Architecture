using Microsoft.SemanticKernel;
using Ollama_Component.Services.ChatService.Models;

namespace Ollama_Component.Services.ChatService
{
    public interface IChatService
    {
        Task<EndpointChatResponse> GetModelResponse(PromptRequest request);
        IAsyncEnumerable<OllamaModelResponse> GetStreamedModelResponse(PromptRequest request);

    }
}