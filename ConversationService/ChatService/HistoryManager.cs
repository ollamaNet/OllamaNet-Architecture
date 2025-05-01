using ConversationService.Cache;
using ConversationService.ChatService.DTOs;
using ConversationService.ChatService.Mappers;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Ollama_DB_layer.UOW;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ConversationService.ChatService
{
   public class ChatHistoryManager
   {
       private readonly ILogger<ChatHistoryManager> _logger;
       private readonly IUnitOfWork _unitOfWork;
       private readonly ICacheManager _cacheManager;

       public ChatHistoryManager(
           ILogger<ChatHistoryManager> logger, 
           IUnitOfWork unitOfWork,
           ICacheManager cacheManager)
       {
           _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
           _logger = logger ?? throw new ArgumentNullException(nameof(logger));
           _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
       }







       /// <summary>
       /// Retrieves chat history for a given conversation ID with caching support.
       /// </summary>
       public async Task<ChatHistory> GetChatHistoryWithCachingAsync(PromptRequest request)
       {
           if (request == null)
           {
               _logger.LogWarning("Invalid request: Conversation ID is missing");
               throw new ArgumentNullException(nameof(request));
           }

           var stopwatch = Stopwatch.StartNew();
           var cacheKey = string.Format(CacheKeys.ChatHistory, request.ConversationId);

           try
           {
               return await _cacheManager.GetOrSetAsync(
                   cacheKey,
                   async () => 
                   {
                       _logger.LogInformation("Cache miss for chat history, loading from database for conversation: {ConversationId}", 
                           request.ConversationId);
                       
                       return await LoadChatHistoryFromDatabaseAsync(request);
                   },
                   TimeSpan.FromMinutes(30) // Cache for 30 minutes
               );
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error retrieving chat history for conversation: {ConversationId}", 
                   request.ConversationId);
               throw;
           }
           finally
           {
               stopwatch.Stop();
               _logger.LogInformation("Retrieved chat history in {ElapsedMilliseconds}ms for conversation: {ConversationId}", 
                   stopwatch.ElapsedMilliseconds, request.ConversationId);
           }
       }








       /// <summary>
       /// Retrieves chat history for a given conversation ID directly from the database.
       /// Returns null if no conversation is found.
       /// </summary>
       private async Task<ChatHistory> LoadChatHistoryFromDatabaseAsync(PromptRequest request)
       {
           var conv = await _unitOfWork.ConversationRepo.GetByIdAsync(request.ConversationId);

           if (conv == null)
           {
               _logger.LogWarning("No conversation found with ID: {ConversationId}", request.ConversationId);
               throw new InvalidOperationException($"No conversation found with ID: {request.ConversationId}");
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
                       _logger.LogWarning("Unknown message role: {Role} in conversation {ConversationId}", 
                           message.Role, request.ConversationId);
                       break;
               }
           }

           chatHistory.AddSystemMessage(conv.SystemMessage);
           _logger.LogInformation("Retrieved {MessageCount} messages for conversation ID: {ConversationId}", 
               chatHistory.Count, request.ConversationId);

           return chatHistory;
       }










       /// <summary>
       /// Gets chat history directly from database without caching.
       /// This is maintained for backward compatibility.
       /// </summary>
       public async Task<ChatHistory?> GetChatHistoryAsync(PromptRequest request)
       {
           if (request == null)
           {
               _logger.LogWarning("Invalid request: Conversation ID is missing");
               return null;
           }

           try
           {
               return await LoadChatHistoryFromDatabaseAsync(request);
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error retrieving chat history from database for conversation: {ConversationId}", 
                   request.ConversationId);
               throw;
           }
       }






       /// <summary>
       /// Updates the chat history in the cache.
       /// </summary>
       public async Task UpdateChatHistoryInCacheAsync(string conversationId, ChatHistory chatHistory)
       {
           if (string.IsNullOrEmpty(conversationId))
           {
               _logger.LogWarning("Cannot update chat history: Conversation ID is missing");
               return;
           }

           if (chatHistory == null)
           {
               _logger.LogWarning("Cannot update chat history: Chat history is null for conversation: {ConversationId}", 
                   conversationId);
               return;
           }

           var cacheKey = string.Format(CacheKeys.ChatHistory, conversationId);

           try
           {
               await _cacheManager.GetOrSetAsync(
                   cacheKey,
                   async () => 
                   {
                       return chatHistory;
                   },
                   TimeSpan.FromMinutes(30) // Cache for 30 minutes
               );

               _logger.LogInformation("Updated chat history in cache for conversation: {ConversationId}", conversationId);
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error updating chat history in cache for conversation: {ConversationId}", 
                   conversationId);
           }
       }







       /// <summary>
       /// Invalidates the chat history cache for a conversation.
       /// </summary>
       public async Task InvalidateChatHistoryCacheAsync(string conversationId)
       {
           if (string.IsNullOrEmpty(conversationId))
           {
               _logger.LogWarning("Cannot invalidate chat history: Conversation ID is missing");
               return;
           }

           var cacheKey = string.Format(CacheKeys.ChatHistory, conversationId);

           try
           {
               await _cacheManager.InvalidateCache(cacheKey);
               _logger.LogInformation("Invalidated chat history cache for conversation: {ConversationId}", conversationId);
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error invalidating chat history cache for conversation: {ConversationId}", 
                   conversationId);
           }
       }








       /// <summary>
       /// Saves the user prompt and AI response into the database and updates the cache.
       /// Returns true if successful, false otherwise.
       /// </summary>
       public async Task<bool> SaveStreamedChatInteractionAsync(PromptRequest request, List<OllamaModelResponse> response, ChatHistory updatedHistory)
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

               // Update cache with the new history
               if (updatedHistory != null)
               {
                   await UpdateChatHistoryInCacheAsync(request.ConversationId, updatedHistory);
               }

               return true;
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error saving chat interaction for conversation ID {ConversationId}", 
                   request.ConversationId);
               return false;
           }
       }








       /// <summary>
       /// Saves chat interaction data, including prompts and AI responses, and updates the cache.
       /// Returns true if successful, false otherwise.
       /// </summary>
       public async Task<bool> SaveChatInteractionAsync(PromptRequest request, IReadOnlyList<OllamaModelResponse> response, ChatHistory updatedHistory)
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

               // Update cache with the new history
               if (updatedHistory != null)
               {
                   await UpdateChatHistoryInCacheAsync(request.ConversationId, updatedHistory);
               }

               return true;
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error saving chat interaction for conversation ID {ConversationId}", 
                   request.ConversationId);
               return false;
           }
       }
   }
}
