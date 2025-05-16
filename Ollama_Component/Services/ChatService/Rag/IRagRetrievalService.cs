using Microsoft.Identity.Client;

namespace Ollama_Component.Services.ChatService.DTOs
{

    public interface IRagRetrievalService
    {
        Task<List<string>> GetRelevantContextAsync(PromptRequest request);

    }

}

