using Models;

namespace ChatService
{
    public interface ISemanticKernelService
    {
        Task<string> SendMessageAsync(PromptRequest request);
    }
}