using ConversationService.ChatService.DTOs;
using OllamaSharp.Models.Chat;
using System;

namespace ConversationService.ChatService.Mappers
{
    public static class ModelResponseMapper
    {
        public static ChatResponse ToModelResponse(this OllamaModelResponse ollamaResponse, PromptRequest prompt)
        {
            if (ollamaResponse == null) throw new ArgumentNullException(nameof(ollamaResponse));

            ChatResponse response = new()
            {
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
