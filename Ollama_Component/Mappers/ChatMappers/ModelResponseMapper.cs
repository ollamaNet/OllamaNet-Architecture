using Microsoft.SemanticKernel.ChatCompletion;
using Ollama_Component.Services.ChatService.Models;
using OllamaSharp.Models.Chat;

namespace Ollama_Component.Mappers.ChatMappers
{
    public static class ModelResponseMapper
    {
        public static EndpointChatResponse ToModelResponse(this ModelResponse ollamaResponse, PromptRequest prompt)
        {
            if (ollamaResponse == null) throw new ArgumentNullException(nameof(ollamaResponse));


            EndpointChatResponse response = new()
            {
                UserId = prompt.UserId,
                ConversationId = prompt.ConversationId,
                ResposneId = Guid.NewGuid().ToString(),
                Content = ollamaResponse.Content,
                ModelName = ollamaResponse.ModelId,
                TotalDuration = ollamaResponse.TotalDuration,
                LoadDuration = ollamaResponse.LoadDuration,
                PromptEvalCount = ollamaResponse.PromptEvalCount,
                PromptEvalDuration = ollamaResponse.PromptEvalDuration,
                EvalCount = ollamaResponse.EvalCount,
                EvalDuration = ollamaResponse.EvalDuration,
                CreatedAt = ollamaResponse.CreatedAt,
            };

            return response;
        }
    }
}
