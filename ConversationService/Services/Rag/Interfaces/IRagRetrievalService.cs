using ConversationServices.Services.ChatService.DTOs;

namespace ConversationService.Services.Rag.Interfaces
{
    public interface IRagRetrievalService
    {
        Task<List<string>> GetRelevantContextAsync(PromptRequest request);
    }
} 