using Microsoft.Extensions.Logging;
using Ollama_DB_layer.Entities;
using Ollama_DB_layer.Repositories;
using Ollama_DB_layer.Repositories.AIResponseRepo;
using Ollama_DB_layer.Repositories.ConversationRepo;
using Ollama_DB_layer.Repositories.ConversationUserPromptRepo;
using Ollama_DB_layer.Repositories.PromptRepo;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ollama_Component.Services.ChatService.Models;
using OllamaSharp.Models.Chat;

namespace Ollama_Component.Services.ChatService
{
    public class ChatHistoryManager
    {
        private readonly IConversationRepository _conversationRepo;
        private readonly IPromptRepository _promptRepo;
        private readonly IAIResponseRepository _responseRepo;
        private readonly IConversationPromptResponseRepository _convPromptResRepo;
        private readonly ILogger<ChatHistoryManager> _logger;

        public ChatHistoryManager(
            IConversationRepository conversationRepo,
            IPromptRepository promptRepo,
            IAIResponseRepository responseRepo,
            IConversationPromptResponseRepository convPromptResRepo,
            ILogger<ChatHistoryManager> logger)
        {
            _conversationRepo = conversationRepo;
            _promptRepo = promptRepo;
            _responseRepo = responseRepo;
            _convPromptResRepo = convPromptResRepo;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves chat history for a given conversation ID.
        /// </summary>
        public async Task<ChatHistory> GetChatHistoryAsync(PromptRequest request)
        {
            var conv = await _conversationRepo.GetByIdAsync(request.ConversationId);

            ChatHistory chatHistory = new ChatHistory();

            if (conv == null)
            {
                _logger.LogWarning("No conversation found with the given ID: {ConversationId}. Creating a new conversation.", request.ConversationId);

                // Create a new conversation
                conv = DbMappers.ToConversation(request);
                await _conversationRepo.AddAsync(conv);
                await _conversationRepo.SaveChangesAsync();

                // Add system and user messages to chat history
                chatHistory.AddSystemMessage(request.SystemMessage);
                chatHistory.AddUserMessage(request.Content);

                _logger.LogInformation("New conversation created with ID: {ConversationId}. Initial messages added.", request.ConversationId);
            }
            else
            {
                //foreach (var message in conv.Messages)
                //{
                //    chatHistory.AddMessage(message.Role, message.Content);
                //}

                _logger.LogInformation("Retrieved {MessageCount} messages for conversation ID: {ConversationId}", chatHistory.Count, request.ConversationId);
            }

            return chatHistory;
        }



        /// <summary>
        /// Saves the user prompt and AI response into the database.
        /// </summary>
        public async Task SaveChatInteractionAsync(PromptRequest request, IReadOnlyList<ModelResponse> response)
        {
            try
            {
                var repoResponse = DbMappers.ToAIResponse(response);
                var repoPrompt = DbMappers.ToPrompt(request);
                var repoConvPromptRes = DbMappers.ToConversationPromptResponse(request, repoPrompt, repoResponse);


                await _responseRepo.AddAsync(repoResponse);
                await _promptRepo.AddAsync(repoPrompt);
                //Pending Edit
                await _convPromptResRepo.AddAsync(request.ConversationId);

                await _promptRepo.SaveChangesAsync();
                await _responseRepo.SaveChangesAsync();
                await _convPromptResRepo.SaveChangesAsync();

                _logger.LogInformation("Saved chat interaction: Prompt ID {PromptId}, Response ID {ResponseId}",
                    repoPrompt.Id, repoResponse.Id);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving chat interaction for conversation ID {ConversationId}", request.ConversationId);
                throw;
            }
        }
    }
}
