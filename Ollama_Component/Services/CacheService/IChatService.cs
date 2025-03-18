using Microsoft.SemanticKernel;
using Ollama_Component.Services.ChatService.DTOs;

namespace Ollama_Component.Services.ChatService
{
    public interface IChatService
    {
        Task<ChatResponse> GetModelResponse(PromptRequest request);
        IAsyncEnumerable<OllamaModelResponse> GetStreamedModelResponse(PromptRequest request);

    }
}