namespace Ollama_Component.Services.ChatService.DTOs
{
    public interface IRagIndexingService
    {
        Task IndexDocumentAsync(PromptRequest request);
    }
}
