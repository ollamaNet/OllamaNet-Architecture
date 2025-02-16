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
using Ollama_DB_layer.Helpers;
using Ollama_DB_layer.UOW;

namespace Ollama_Component.Services.ChatService
{
    public class ChatHistoryManager
    {
        private readonly ILogger<ChatHistoryManager> _logger;
        private readonly AddMessages _addMessages;
        private readonly IUnitOfWork _unitOfWork;


        public ChatHistoryManager(ILogger<ChatHistoryManager> logger, AddMessages addMessages, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _addMessages = addMessages;
        }

        /// <summary>
        /// Retrieves chat history for a given conversation ID.
        /// </summary>
        public async Task<ChatHistory> GetChatHistoryAsync(PromptRequest request)
        {
            var conv = await _unitOfWork.ConversationRepo.GetByIdAsync(request.ConversationId);

            ChatHistory chatHistory = new ChatHistory();

            if (conv == null)
            {
                _logger.LogWarning("No conversation found with the given ID: {ConversationId}. Creating a new conversation.", request.ConversationId);

                // Create a new conversation
                conv = HistoryMapper.ToConversation(request);
                await _unitOfWork.ConversationRepo.AddAsync(conv);
                await _unitOfWork.SaveChangesAsync();

                // Add system and user messages to chat history
                chatHistory.AddSystemMessage(request.SystemMessage);
                chatHistory.AddUserMessage(request.Content);

                _logger.LogInformation("New conversation created with ID: {ConversationId}. Initial messages added.", request.ConversationId);
            }
            else
            {
                var messages = await _unitOfWork.MessageHistoryRepo.GetMessagesByConversationIdAsync(request.ConversationId);

                foreach (var message in messages)
                {
                    if (message.Role == "Prompt")
                    {
                        chatHistory.AddUserMessage(message.Content);
                    }
                    else if (message.Role == "AIResponse")
                    {
                        chatHistory.AddAssistantMessage(message.Content);
                    }
                }

                chatHistory.AddSystemMessage(conv.SystemMessage);
                _logger.LogInformation("Retrieved {MessageCount} messages for conversation ID: {ConversationId}", chatHistory.Count, request.ConversationId);
            }

            return chatHistory;
        }


        /// <summary>
        /// Saves the user prompt and AI response into the database.
        /// </summary>
        public async Task SaveStreamedChatInteractionAsync(PromptRequest request, List<OllamaModelResponse> response)
        {
            try
            {
                var repoResponse = HistoryMapper.ToStreamedAIResponse(response);
                var repoPrompt = HistoryMapper.ToPrompt(request);
                var repoConvPromptRes = HistoryMapper.ToConversationPromptResponse(request, repoPrompt, repoResponse);


                await _addMessages.AddAsync(repoPrompt, repoResponse, repoConvPromptRes);

                _logger.LogInformation("Saved chat interaction: Prompt ID {PromptId}, Response ID {ResponseId}",
                    repoPrompt.Id, repoResponse.Id);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving chat interaction for conversation ID {ConversationId}", request.ConversationId);
                throw;
            }
        }

        public async Task SaveChatInteractionAsync(PromptRequest request, IReadOnlyList<OllamaModelResponse> response)
        {
            try
            {
                var repoResponse = HistoryMapper.ToAIResponse(response);
                var repoPrompt = HistoryMapper.ToPrompt(request);
                var repoConvPromptRes = HistoryMapper.ToConversationPromptResponse(request, repoPrompt, repoResponse);


                await _addMessages.AddAsync(repoPrompt, repoResponse, repoConvPromptRes);

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
