using ConversationService.ConversationService.DTOs;
using Ollama_DB_layer.DataBaseHelpers;
using Ollama_DB_layer.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConversationService.ConversationService
{
    public interface IConversationService
    {
        Task<OpenConversationResponse> CreateConversationAsync(string userId, OpenConversationRequest request);
        
        Task<PagedResult<Conversation>> GetConversationsAsync(string userId, int pageNumber = 1, int pageSize = 15);
        
        Task<PagedResult<Conversation>> SearchConversationsAsync(string userId, string searchTerm, int pageNumber = 1, int pageSize = 15);
        
        Task<GetConversationInfoResponse> GetConversationInfoAsync(string conversationId);
        
        Task<List<MessageHistory>> GetConversationMessagesAsync(string conversationId);
        
        Task<bool> UpdateConversationAsync(string conversationId, UpdateConversationRequest request);
        
        Task<bool> DeleteConversationAsync(string conversationId);
    }
}