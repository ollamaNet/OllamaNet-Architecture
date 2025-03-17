using Microsoft.Extensions.Logging;
using Ollama_DB_layer.UOW;
using Ollama_Component.Services.ChatService.DTOs;
using System;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Ollama_Component.Services.ChatService
{
    public class ChatHistoryManager
    {
        private readonly ILogger<ChatHistoryManager> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public ChatHistoryManager(ILogger<ChatHistoryManager> logger, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves chat history for a given conversation ID.
        /// Returns null if no conversation is found.
        /// </summary>
        public async Task<ChatHistory?> GetChatHistoryAsync(PromptRequest request)
        {
            if (request == null )
            {
                _logger.LogWarning("Invalid request: Conversation ID is missing or invalid.");
                return null;
            }

            var conv = await _unitOfWork.ConversationRepo.GetByIdAsync(request.ConversationId);
            if (conv == null)
            {
                _logger.LogWarning("No conversation found with ID: {ConversationId}", request.ConversationId);
                return null;
            }

            var messages = await _unitOfWork.GetHistoryRepo.GetHistoryForAIAsync(request.ConversationId);
            var chatHistory = new ChatHistory();

            foreach (var message in messages)
            {
                switch (message.Role)
                {
                    case "user":
                        chatHistory.AddUserMessage(message.Content);
                        break;
                    case "Assistant":
                        chatHistory.AddAssistantMessage(message.Content);
                        break;
                    default:
                        _logger.LogWarning("Unknown message role: {Role} in conversation {ConversationId}", message.Role, request.ConversationId);
                        break;
                }
            }

            chatHistory.AddSystemMessage(conv.SystemMessage);
            _logger.LogInformation("Retrieved {MessageCount} messages for conversation ID: {ConversationId}", chatHistory.Count, request.ConversationId);

            return chatHistory;
        }

        /// <summary>
        /// Saves the user prompt and AI response into the database.
        /// Returns true if successful, false otherwise.
        /// </summary>
        public async Task<bool> SaveStreamedChatInteractionAsync(PromptRequest request, List<OllamaModelResponse> response)
        {
            if (request == null || response == null || response.Count == 0)
            {
                _logger.LogWarning("Invalid request: Request or response is null/empty.");
                return false;
            }

            try
            {
                var repoResponse = HistoryMapper.ToStreamedAIResponse(response);
                var repoPrompt = HistoryMapper.ToPrompt(request);
                var repoConvPromptRes = HistoryMapper.ToConversationPromptResponse(request, repoPrompt, repoResponse);

               
                await _unitOfWork.SetHistoryRepo.SetHistoryAsync(repoPrompt, repoResponse, repoConvPromptRes);

                _logger.LogInformation("Saved chat interaction: Prompt ID {PromptId}, Response ID {ResponseId}",
                    repoPrompt.Id, repoResponse.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving chat interaction for conversation ID {ConversationId}", request.ConversationId);
                return false;
            }
        }

        /// <summary>
        /// Saves chat interaction data, including prompts and AI responses.
        /// Returns true if successful, false otherwise.
        /// </summary>
        public async Task<bool> SaveChatInteractionAsync(PromptRequest request, IReadOnlyList<OllamaModelResponse> response)
        {
            if (request == null || response == null || response.Count == 0)
            {
                _logger.LogWarning("Invalid request: Request or response is null/empty.");
                return false;
            }

            try
            {
                var repoResponse = HistoryMapper.ToAIResponse(response);
                var repoPrompt = HistoryMapper.ToPrompt(request);
                var repoConvPromptRes = HistoryMapper.ToConversationPromptResponse(request, repoPrompt, repoResponse);

                await _unitOfWork.SetHistoryRepo.SetHistoryAsync(repoPrompt, repoResponse, repoConvPromptRes);

                _logger.LogInformation("Saved chat interaction: Prompt ID {PromptId}, Response ID {ResponseId}",
                    repoPrompt.Id, repoResponse.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving chat interaction for conversation ID {ConversationId}", request.ConversationId);
                return false;
            }
        }
    }
}
