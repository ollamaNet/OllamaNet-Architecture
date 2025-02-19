using Ollama_Component.Services.ConversationService.Models;

namespace Ollama_Component.Services.ConversationService
{
    public interface IConversationService
    {
        Task<OpenConversationResponse> CreateConversationAsync(OpenConversationRequest request);
    }
}