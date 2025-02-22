using Ollama_Component.Services.ConversationService.Models;
using Ollama_DB_layer.DataBaseHelpers;
using Ollama_DB_layer.Entities;

namespace Ollama_Component.Services.ConversationService
{
    public interface IConversationService
    {
        Task<OpenConversationResponse> CreateConversationAsync(OpenConversationRequest request);
        Task<PagedResult<Conversation>> GetConversationsAsync(string UserId);
        Task<GetConversationInfoResponse> GetConversationInfoAsync(string ConversationId);
        Task<List<History>> GetConversationMessagesAsync(string conversationId);

    }
}