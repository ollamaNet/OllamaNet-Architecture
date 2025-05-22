using ConversationService.ChatService.DTOs;

namespace ConversationService.ChatService.RagService
{
    public interface IRagRetrievalService
    {
        Task<List<string>> GetRelevantContextAsync(PromptRequest request);
    }
}
