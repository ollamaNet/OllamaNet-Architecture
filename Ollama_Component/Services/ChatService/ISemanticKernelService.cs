using Microsoft.SemanticKernel;
using Models;

namespace Ollama_Component.Services.ChatService
{
    public interface ISemanticKernelService
    {
        Task<string> GetModelResponse(PromptRequest request);
        Task<IAsyncEnumerable<StreamingChatMessageContent>> GetStreamingModelResponse(PromptRequest request);



    }
}