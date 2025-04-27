using ConversationService.Cache;
using ConversationService.ChatService.DTOs;
using ConversationService.ChatService.Mappers;
using ConversationService.Connectors;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ConversationService.ChatService
{
   public class ChatService : IChatService
   {
       private readonly IOllamaConnector _connector;
       private readonly ILogger<ChatService> _logger;
       private readonly ChatHistoryManager _chatHistoryManager;

       public ChatService(
           IOllamaConnector connector,
           ILogger<ChatService> logger,
           ChatHistoryManager chatHistoryManager)
       {
           _connector = connector ?? throw new ArgumentNullException(nameof(connector));
           _logger = logger ?? throw new ArgumentNullException(nameof(logger));
           _chatHistoryManager = chatHistoryManager ?? throw new ArgumentNullException(nameof(chatHistoryManager));
       }








       /// <summary>
       /// Gets a streamed model response for a prompt request
       /// </summary>
       public IAsyncEnumerable<OllamaModelResponse> GetStreamedModelResponse(PromptRequest request)
       {
           if (request == null)
               throw new ArgumentException("Request cannot be null", nameof(request));

           if (string.IsNullOrEmpty(request.ConversationId))
               throw new ArgumentException("Conversation ID is required", nameof(request));

           // Log the operation start
           _logger.LogInformation("Processing streamed chat request for conversation: {ConversationId}, model: {Model}",
               request.ConversationId, request.Model);

           // Return an async enumerable without using try/catch at this level
           return ProcessStreamedResponseAsync(request);
       }








       /// <summary>
       /// Internal method to process the streaming response without yielding in a try-catch
       /// </summary>
       private async IAsyncEnumerable<OllamaModelResponse> ProcessStreamedResponseAsync(
           PromptRequest request,
           [EnumeratorCancellation] System.Threading.CancellationToken cancellationToken = default)
       {
           var stopwatch = Stopwatch.StartNew();
           List<OllamaModelResponse> responses = new List<OllamaModelResponse>();
           ChatHistory history = null;

           try
           {
               // Get chat history from cache or database
               history = await _chatHistoryManager.GetChatHistoryWithCachingAsync(request);
               _logger.LogInformation("Chat history retrieved for conversation: {ConversationId}", request.ConversationId);

               // Add latest user message and system message to chat history
               if (!string.IsNullOrEmpty(request.SystemMessage))
               {
                   history.AddSystemMessage(request.SystemMessage);
               }
               history.AddUserMessage(request.Content);
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error preparing chat history for conversation: {ConversationId}", request.ConversationId);
               throw; // Rethrow to caller
           }

           // Stream responses from the model
           await foreach (var response in _connector.GetStreamedChatMessageContentsAsync(history, request)
               .WithCancellation(cancellationToken))
           {
               // Skip null responses
               if (response == null)
               {
                   continue;
               }

               // Add assistant message to history for tracking
               if (!string.IsNullOrEmpty(response.Content))
               {
                   history.AddAssistantMessage(response.Content);
               }

               responses.Add(response);
               yield return response;
           }

           try
           {
               // After all streaming is done, save the interaction
               stopwatch.Stop();
               _logger.LogInformation("Streaming completed in {ElapsedMilliseconds}ms for conversation: {ConversationId}", 
                   stopwatch.ElapsedMilliseconds, request.ConversationId);

               // Use Task.Run to not block the streaming
               _ = Task.Run(async () =>
               {
                   try
                   {
                       await _chatHistoryManager.SaveStreamedChatInteractionAsync(request, responses, history);
                       _logger.LogInformation("Saved streamed chat interaction for conversation: {ConversationId}", 
                           request.ConversationId);
                   }
                   catch (Exception ex)
                   {
                       _logger.LogError(ex, "Error saving streamed chat interaction for conversation: {ConversationId}", 
                           request.ConversationId);
                   }
               });
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error finalizing streaming for conversation: {ConversationId}", 
                   request.ConversationId);
               // Don't throw here since we're after yielding all responses
           }
       }












       /// <summary>
       /// Gets a model response for a prompt request (non-streaming)
       /// </summary>
       public async Task<ChatResponse> GetModelResponse(PromptRequest request)
       {
           if (request == null)
               throw new ArgumentException("Request cannot be null", nameof(request));

           if (string.IsNullOrEmpty(request.ConversationId))
               throw new ArgumentException("Conversation ID is required", nameof(request));

           var stopwatch = Stopwatch.StartNew();

           // Log the operation start
           _logger.LogInformation("Processing chat request for conversation: {ConversationId}, model: {Model}",
               request.ConversationId, request.Model);

           // Get chat history from cache or database
           ChatHistory history;
           try
           {
               // Use the enhanced method with caching
               history = await _chatHistoryManager.GetChatHistoryWithCachingAsync(request);
               _logger.LogInformation("Chat history retrieved for conversation: {ConversationId}", request.ConversationId);
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error retrieving chat history for conversation: {ConversationId}", request.ConversationId);
               throw;
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
               var ollamaResponses = await _connector.GetChatMessageContentsAsync(history, request);

               if (ollamaResponses == null || ollamaResponses.Count == 0 || ollamaResponses[0] == null)
               {
                   _logger.LogWarning("Model returned empty response for conversation: {ConversationId}", request.ConversationId);
                   return new ChatResponse
                   {
                       ConversationId = request.ConversationId,
                       ResposneId = Guid.NewGuid().ToString(),
                       Content = string.Empty,
                       CreatedAt = DateTime.UtcNow
                   };
               }

               // Map response
               var response = ModelResponseMapper.ToModelResponse(ollamaResponses[0], request);

               // Add LLM response to history and save history to cache
               history.AddAssistantMessage(ollamaResponses[0].Content ?? string.Empty);

               // Save chat interaction to database and update cache
               await _chatHistoryManager.SaveChatInteractionAsync(request, ollamaResponses, history);

               stopwatch.Stop();
               _logger.LogInformation("Processed chat request successfully in {ElapsedMilliseconds}ms for conversation: {ConversationId}",
                   stopwatch.ElapsedMilliseconds, request.ConversationId);

               return response;
           }
           catch (Exception ex)
           {
               stopwatch.Stop();
               _logger.LogError(ex, "Error getting model response for conversation: {ConversationId} after {ElapsedMilliseconds}ms",
                   request.ConversationId, stopwatch.ElapsedMilliseconds);
               throw;
           }
       }
   }
}
