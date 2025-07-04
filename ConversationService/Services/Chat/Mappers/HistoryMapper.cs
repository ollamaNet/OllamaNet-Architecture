﻿using ConversationServices.Services.ChatService.DTOs;
using Ollama_DB_layer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConversationServices.Services.ChatService.Mappers
{
    public static class HistoryMapper
    {
        public static AIResponse ToStreamedAIResponse(this List<OllamaModelResponse> response)
        {
            return new AIResponse
            {
                Content = string.Join("", response.Select(r => r.Content)),
                Role = response[0].Role.ToString(),
                TotalDuration = response[0].TotalDuration.ToString(),
                LoadDuration = response[0].LoadDuration.ToString(),
                PromptEvalCount = response[0].PromptEvalCount.ToString(),
                PromptEvalDuration = response[0].PromptEvalDuration.ToString(),
                EvalCount = response[0].EvalCount.ToString(),
                EvalDuration = response[0].EvalDuration.ToString(),
                CreatedAt = DateTime.UtcNow
            };
        }

        public static AIResponse ToAIResponse(this IReadOnlyList<OllamaModelResponse> response)
        {
            return new AIResponse
            {
                Content = response[0].Content,
                Role = response[0].Role.ToString(),
                TotalDuration = response[0].TotalDuration.ToString(),
                LoadDuration = response[0].LoadDuration.ToString(),
                PromptEvalCount = response[0].PromptEvalCount.ToString(),
                PromptEvalDuration = response[0].PromptEvalDuration.ToString(),
                EvalCount = response[0].EvalCount.ToString(),
                EvalDuration = response[0].EvalDuration.ToString(),
                CreatedAt = DateTime.UtcNow
            };
        }

        public static Prompt ToPrompt(this PromptRequest request)
        {
            Prompt prompt = new()
            {
                Content = request.Content,
                CreatedAt = request.CreatedAt,
            };

            if(request.Options != null)
            {
                if(request.Options.Temperature != null)
                    prompt.Temprature = request.Options.Temperature.ToString();
            }
            return prompt;
        }

        public static ConversationPromptResponse ToConversationPromptResponse(this PromptRequest request, Prompt repoPrompt, AIResponse repoResponse)
        {
            return new ConversationPromptResponse
            {
                Conversation_Id = request.ConversationId,
                Prompt_Id = repoPrompt.Id,
                Response_Id = repoResponse.Id
            };
        }
    }
}