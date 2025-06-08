using ConversationServices.Services.ChatService.DTOs;

namespace ConversationService.Services.Rag.Interfaces
{
    public interface IRagIndexingService
    {
        Task IndexDocumentAsync(PromptRequest request);
    }
} 