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
        private readonly IConversationRepository _conversationRepo;
        private readonly IPromptRepository _promptRepo;
        private readonly IAIResponseRepository _responseRepo;
        private readonly IConversationPromptResponseRepository _convPromptResRepo;
        private readonly ILogger<ChatHistoryManager> _logger;
        private readonly GetMessages _getmessages;
        private readonly AddMessages _addMessages;

        private readonly IUnitOfWork _unitOfWork;


        public ChatHistoryManager(
            IConversationRepository conversationRepo,
            IPromptRepository promptRepo,
            IAIResponseRepository responseRepo,
            IConversationPromptResponseRepository convPromptResRepo,
            ILogger<ChatHistoryManager> logger,
            GetMessages getmessages,
            AddMessages addMessages,
            IUnitOfWork unitOfWork)
        {
            _conversationRepo = conversationRepo;
            _promptRepo = promptRepo;
            _responseRepo = responseRepo;
            _convPromptResRepo = convPromptResRepo;
            _logger = logger;
            _getmessages = getmessages;
            _unitOfWork = unitOfWork;
            _addMessages = addMessages;
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
                conv = HistoryMapper.ToConversation(request);
                await _conversationRepo.AddAsync(conv);
                

                // Add system and user messages to chat history
                chatHistory.AddSystemMessage(request.SystemMessage);
                chatHistory.AddUserMessage(request.Content);

                _logger.LogInformation("New conversation created with ID: {ConversationId}. Initial messages added.", request.ConversationId);
            }
            else
            {
                var messages = await _getmessages.GetMessagesByConversationIdAsync(request.ConversationId);
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
                    else if (message.Role == "SYSTEM")
                    {
                        chatHistory.AddSystemMessage(message.Content);
                    }
                }

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
