using Microsoft.SemanticKernel;
using Models;

namespace ChatService
{
    public interface ISemanticKernelService
    {
        Task<string> GetModelResponse(PromptRequest request);
        Task<IAsyncEnumerable<StreamingChatMessageContent>> GetStreamingModelResponse(PromptRequest request);



    }
}