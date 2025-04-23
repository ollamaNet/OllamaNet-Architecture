using ConversationService.Cache;
using ConversationService.ChatService.DTOs;
using ConversationService.ChatService.Mappers;
using ConversationService.Connectors;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConversationService.ChatService
{
   public class ChatService : IChatService
   {
       private readonly IOllamaConnector _connector;
       private readonly ILogger<ChatService> _logger;
       private readonly ChatHistoryManager _chatHistoryManager;
       private readonly ICacheManager _cacheManager;

       public ChatService(
           IOllamaConnector connector,
           ILogger<ChatService> logger,
           ChatHistoryManager chatHistoryManager,
           ICacheManager cacheManager)
       {
           _connector = connector ?? throw new ArgumentNullException(nameof(connector));
           _logger = logger ?? throw new ArgumentNullException(nameof(logger));
           _chatHistoryManager = chatHistoryManager ?? throw new ArgumentNullException(nameof(chatHistoryManager));
           _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
       }

       public async IAsyncEnumerable<OllamaModelResponse> GetStreamedModelResponse(PromptRequest request)
       {
           if (request == null)
               throw new ArgumentException("Request cannot be null", nameof(request));

           if (string.IsNullOrEmpty(request.ConversationId))
               throw new ArgumentException("Conversation ID is required", nameof(request));

           // Log the operation start
           _logger.LogInformation("Processing streamed chat request for conversation: {ConversationId}, model: {Model}",
               request.ConversationId, request.Model);

           // Get chat history from cache or database
           var (found, history) = await _cacheManager.GetChatHistoryAsync(request.ConversationId);

           if (found)
           {
               _logger.LogInformation("Chat history retrieved from cache for conversation: {ConversationId}",
                   request.ConversationId);
           }
           else
           {
               _logger.LogInformation("Chat history not found in cache, retrieving from database for conversation: {ConversationId}",
                   request.ConversationId);

               try
               {
                   history = await _chatHistoryManager.GetChatHistoryAsync(request);
               }
               catch (Exception ex)
               {
                   _logger.LogError(ex, "Error retrieving chat history from database for conversation: {ConversationId}",
                       request.ConversationId);
                   throw;
               }
           }

           // Add latest user message and system message to chat history
           if (!string.IsNullOrEmpty(request.SystemMessage))
           {
               history.AddSystemMessage(request.SystemMessage);
           }
           history.AddUserMessage(request.Content);

           // Stream responses from the model
           List<OllamaModelResponse> responses = new List<OllamaModelResponse>();

           try
           {
               await foreach (var response in _connector.GetStreamedChatMessageContentsAsync(history, request))
               {
                   // Skip null responses
                   if (response == null)
                   {
                       continue;
                   }

                   // Add assistant message to history and cache it
                   if (!string.IsNullOrEmpty(response.Content))
                   {
                       // Add to history for tracking
                       history.AddAssistantMessage(response.Content);

                       // Update cache periodically during streaming
                       await _cacheManager.SetChatHistoryAsync(request.ConversationId, history);
                   }

                   responses.Add(response);
                   yield return response;
               }

               // Final cache update after all responses
               await _cacheManager.SetChatHistoryAsync(request.ConversationId, history);

               // Save to database in background
               _ = Task.Run(async () =>
               {
                   try
                   {
                       await _chatHistoryManager.SaveStreamedChatInteractionAsync(request, responses);
                       _logger.LogInformation("Saved streamed chat interaction to database for conversation: {ConversationId}",
                           request.ConversationId);
                   }
                   catch (Exception ex)
                   {
                       _logger.LogError(ex, "Error saving streamed chat interaction to database for conversation: {ConversationId}",
                           request.ConversationId);
                   }
               });
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error streaming model response for conversation: {ConversationId}",
                   request.ConversationId);
               throw;
           }
       }

       public async Task<ChatResponse> GetModelResponse(PromptRequest request)
       {
           if (request == null)
               throw new ArgumentException("Request cannot be null", nameof(request));

           if (string.IsNullOrEmpty(request.ConversationId))
               throw new ArgumentException("Conversation ID is required", nameof(request));

           // Log the operation start
           _logger.LogInformation("Processing chat request for conversation: {ConversationId}, model: {Model}",
               request.ConversationId, request.Model);

           // Get chat history from cache or database
           var (found, history) = await _cacheManager.GetChatHistoryAsync(request.ConversationId);

           if (found)
           {
               _logger.LogInformation("Chat history retrieved from cache for conversation: {ConversationId}",
                   request.ConversationId);
           }
           else
           {
               _logger.LogInformation("Chat history not found in cache, retrieving from database for conversation: {ConversationId}",
                   request.ConversationId);

               try
               {
                   history = await _chatHistoryManager.GetChatHistoryAsync(request);
               }
               catch (Exception ex)
               {
                   _logger.LogError(ex, "Error retrieving chat history from database for conversation: {ConversationId}",
                       request.ConversationId);
                   throw;
               }
           }

           // Add latest user message and system message to chat history
           if (!string.IsNullOrEmpty(request.SystemMessage))
           {
               history.AddSystemMessage(request.SystemMessage);
           }
           history.AddUserMessage(request.Content);

           try
           {
               // Get model response
               var ollamaResponse = await _connector.GetChatMessageContentsAsync(history, request);

               if (ollamaResponse.Count == 0 || ollamaResponse[0] == null)
               {
                   _logger.LogWarning("Model returned empty response for conversation: {ConversationId}",
                       request.ConversationId);
                   return new ChatResponse
                   {
                       ConversationId = request.ConversationId,
                       ResposneId = Guid.NewGuid().ToString(),
                       Content = string.Empty,
                       CreatedAt = DateTime.UtcNow
                   };
               }

               // Map response
               var response = ModelResponseMapper.ToModelResponse(ollamaResponse[0], request);

               // Add LLM response to history and save history to cache
               history.AddAssistantMessage(ollamaResponse[0].Content ?? string.Empty);

               // Update cache
               await _cacheManager.SetChatHistoryAsync(request.ConversationId, history);

               // Save to database
               await _chatHistoryManager.SaveChatInteractionAsync(request, ollamaResponse);

               _logger.LogInformation("Processed chat request successfully for conversation: {ConversationId}",
                   request.ConversationId);

               return response;
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error getting model response for conversation: {ConversationId}",
                   request.ConversationId);
               throw;
           }
       }
   }
}
