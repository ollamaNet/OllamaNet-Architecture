
using ConversationServices.Services.ChatService.DTOs;

namespace ConversationService.ChatService.RagService
{
    public interface IRagIndexingService
    {
        Task IndexDocumentAsync(PromptRequest request);
    }
}
