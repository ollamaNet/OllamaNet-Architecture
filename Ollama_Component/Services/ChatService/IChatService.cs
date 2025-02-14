using Microsoft.SemanticKernel;
using Ollama_Component.Services.ChatService.Models;

namespace Ollama_Component.Services.ChatService
{
    public interface IChatService
    {
        Task<IReadOnlyList<ModelResponse>> GetModelResponse(PromptRequest request);
        IAsyncEnumerable<ModelResponse> GetStreamedModelResponse(PromptRequest request);



    }
}