using Microsoft.SemanticKernel;
using Ollama_Component.Services.ChatService.Models;

namespace Ollama_Component.Services.ChatService
{
    public interface IChatService
    {
        Task<string> GetModelResponse(PromptRequest request);
        Task<IAsyncEnumerable<StreamingChatMessageContent>> GetStreamingModelResponse(PromptRequest request);



    }
}