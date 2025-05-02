using ConversationService.Cache;
using ConversationService.ConversationService.DTOs;
using ConversationService.Mappers;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Ollama_DB_layer.DataBaseHelpers;
using Ollama_DB_layer.DTOs;
using Ollama_DB_layer.Entities;
using Ollama_DB_layer.UOW;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ConversationService.Connectors;
using ConversationService.ChatService.DTOs;

namespace ConversationService.ConversationService
{
    public class ConversationService : IConversationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheManager _cacheManager;
        private readonly ILogger<ConversationService> _logger;
        private readonly IOllamaConnector _ollamaConnector;

        public ConversationService(
            IUnitOfWork unitOfWork,
            ICacheManager cacheManager,
            ILogger<ConversationService> logger,
            IOllamaConnector ollamaConnector)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ollamaConnector = ollamaConnector ?? throw new ArgumentNullException(nameof(ollamaConnector));
        }








        public async Task<OpenConversationResponse> CreateConversationAsync(string userId, OpenConversationRequest request)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
                
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Creating new conversation for user: {UserId}, model: {Model}", 
                userId, request.ModelName);
                
            try
            {
                var conversation = ConversationMapper.ToConversationEntity(request, userId);
                
                _unitOfWork.ConversationRepo.AddAsync(conversation);
                await _unitOfWork.SaveChangesAsync();

                var conv = await _unitOfWork.ConversationRepo.GetByIdAsync(conversation.Id);
                
                if (conv == null)
                {
                    _logger.LogError("Failed to retrieve newly created conversation for user: {UserId}", userId);
                    throw new InvalidOperationException("Failed to retrieve newly created conversation");
                }
                
                _logger.LogInformation("Created conversation: {ConversationId} for user: {UserId}", conv.Id, userId);
                
                // Invalidate the user's conversation list cache
                var cacheKey = string.Format(CacheKeys.ConversationList, userId);
                await _cacheManager.InvalidateCache(cacheKey);
                
                var paginatedCacheKey = string.Format(CacheKeys.ConversationListPaginated, userId, "*", "*");
                await _cacheManager.InvalidateCache(paginatedCacheKey);

                stopwatch.Stop();
                _logger.LogInformation("CreateConversationAsync completed in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
                
                return ConversationMapper.ToOpenConversationResponse(conv);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error creating conversation for user: {UserId} after {ElapsedMilliseconds}ms", 
                    userId, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }










        public async Task<PagedResult<ConversationCard>> GetConversationsAsync(string userId, int pageNumber = 1, int pageSize = 15)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            
            if (pageNumber < 1)
                pageNumber = 1;
                
            if (pageSize < 1 || pageSize > 50)
                pageSize = 15;
            
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Retrieving conversations for user: {UserId}, page: {Page}, size: {Size}", 
                userId, pageNumber, pageSize);
            
            try
            {
                // Try to get from cache first with specific pagination parameters
                var cacheKey = string.Format(CacheKeys.ConversationListPaginated, userId, pageNumber, pageSize);
                
                var conversations = await _cacheManager.GetOrSetAsync(
                    cacheKey,
                    async () => 
                    {
                        _logger.LogDebug("Fetching conversations from database for user: {UserId}", userId);
                        return await _unitOfWork.ConversationRepo.ConversationPagination(userId, pageNumber, pageSize);
                    },
                    TimeSpan.FromMinutes(30) // Cache for 30 minutes
                );

                stopwatch.Stop();
                _logger.LogInformation("GetConversationsAsync completed in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
                
                return conversations;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving conversations for user: {UserId} after {ElapsedMilliseconds}ms", 
                    userId, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }





        public async Task<PagedResult<ConversationCard>> SearchConversationsAsync(string userId, string searchTerm, int pageNumber = 1, int pageSize = 15)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
                
            if (string.IsNullOrEmpty(searchTerm))
                return await GetConversationsAsync(userId, pageNumber, pageSize);
            
            if (pageNumber < 1)
                pageNumber = 1;
                
            if (pageSize < 1 || pageSize > 50)
                pageSize = 15;
            
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Searching conversations for user: {UserId}, term: {SearchTerm}, page: {Page}, size: {Size}", 
                userId, searchTerm, pageNumber, pageSize);
            
            try
            {
                // TODO: Implement proper search when repository supports it
                // For now, just return regular pagination and log that search is not yet implemented
                _logger.LogWarning("Search functionality is not yet implemented. Returning regular pagination results.");
                
                var conversations = await GetConversationsAsync(userId, pageNumber, pageSize);
                
                stopwatch.Stop();
                _logger.LogInformation("SearchConversationsAsync completed in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
                
                return conversations;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error searching conversations for user: {UserId}, term: {SearchTerm} after {ElapsedMilliseconds}ms", 
                    userId, searchTerm, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }








        public async Task<GetConversationInfoResponse> GetConversationInfoAsync(string conversationId)
        {
            if (string.IsNullOrEmpty(conversationId))
                throw new ArgumentException("Conversation ID cannot be null or empty", nameof(conversationId));
            
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Retrieving conversation info for: {ConversationId}", conversationId);
            
            try
            {
                // Try to get from cache first
                var cacheKey = string.Format(CacheKeys.ConversationInfo, conversationId);
                
                var response = await _cacheManager.GetOrSetAsync(
                    cacheKey,
                    async () =>
                    {
                        _logger.LogDebug("Fetching conversation info from database for: {ConversationId}", 
                            conversationId);
                            
                        var conversation = await _unitOfWork.ConversationRepo.GetByIdAsync(conversationId);
                        
                        if (conversation == null)
                        {
                            _logger.LogWarning("Conversation not found: {ConversationId}", conversationId);
                            throw new InvalidOperationException($"Conversation not found: {conversationId}");
                        }
                        
                        return ConversationMapper.ToConversationInfoResponse(conversation);
                    },
                    TimeSpan.FromMinutes(30) // Cache for 30 minutes
                );

                stopwatch.Stop();
                _logger.LogInformation("GetConversationInfoAsync completed in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
                
                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving conversation info for: {ConversationId} after {ElapsedMilliseconds}ms", 
                    conversationId, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }










        public async Task<List<MessageHistory>> GetConversationMessagesAsync(string conversationId)
        {
            if (string.IsNullOrEmpty(conversationId))
                throw new ArgumentException("Conversation ID cannot be null or empty", nameof(conversationId));
            
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Retrieving messages for conversation: {ConversationId}", conversationId);
            
            try
            {
                // This data can change frequently, so use a shorter cache duration
                var cacheKey = string.Format(CacheKeys.ConversationMessages, conversationId);
                
                var messages = await _cacheManager.GetOrSetAsync(
                    cacheKey,
                    async () => 
                    {
                        _logger.LogDebug("Fetching messages from database for conversation: {ConversationId}", 
                            conversationId);
                        return await _unitOfWork.GetHistoryRepo.GetHistoryForUserAsync(conversationId);
                    },
                    TimeSpan.FromMinutes(5) // Cache for 5 minutes only
                );

                stopwatch.Stop();
                _logger.LogInformation("GetConversationMessagesAsync completed in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
                
                return messages;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error retrieving messages for conversation: {ConversationId} after {ElapsedMilliseconds}ms", 
                    conversationId, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }





        public async Task<bool> UpdateConversationAsync(string conversationId, UpdateConversationRequest request)
        {
            if (string.IsNullOrEmpty(conversationId))
                throw new ArgumentException("Conversation ID cannot be null or empty", nameof(conversationId));
                
            if (request == null)
                throw new ArgumentNullException(nameof(request));
                
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Updating conversation: {ConversationId}", conversationId);
            
            try
            {
                var conversation = await _unitOfWork.ConversationRepo.GetByIdAsync(conversationId);
                
                if (conversation == null)
                {
                    _logger.LogWarning("Conversation not found for update: {ConversationId}", conversationId);
                    return false;
                }
                
                // Update only provided properties
                bool hasChanges = false;
                
                if (!string.IsNullOrEmpty(request.Title))
                {
                    conversation.Title = request.Title;
                    hasChanges = true;
                }
                
                if (!string.IsNullOrEmpty(request.SystemMessage))
                {
                    conversation.SystemMessage = request.SystemMessage;
                    hasChanges = true;
                }
                
                if (!hasChanges)
                {
                    _logger.LogInformation("No changes provided for conversation: {ConversationId}", conversationId);
                    return true;
                }



                
                // SaveChangesAsync will update the entity since it's being tracked
                await _unitOfWork.SaveChangesAsync();
                
                // Invalidate relevant caches
                var infoKey = string.Format(CacheKeys.ConversationInfo, conversationId);
                await _cacheManager.InvalidateCache(infoKey);
                
                // Invalidate the user's conversation list cache
                var listKey = string.Format(CacheKeys.ConversationList, conversation.User_Id);
                await _cacheManager.InvalidateCache(listKey);
                
                // Invalidate paginated and search caches
                var paginatedKey = string.Format(CacheKeys.ConversationListPaginated, conversation.User_Id, "*", "*");
                await _cacheManager.InvalidateCache(paginatedKey);
                
                var searchKey = string.Format(CacheKeys.ConversationSearch, conversation.User_Id, "*", "*", "*");
                await _cacheManager.InvalidateCache(searchKey);

                stopwatch.Stop();
                _logger.LogInformation("UpdateConversationAsync completed in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
                
                return true;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error updating conversation: {ConversationId} after {ElapsedMilliseconds}ms", 
                    conversationId, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }



        public async Task<bool> DeleteConversationAsync(string conversationId)
        {
            if (string.IsNullOrEmpty(conversationId))
                throw new ArgumentException("Conversation ID cannot be null or empty", nameof(conversationId));
                
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Deleting conversation: {ConversationId}", conversationId);
            
            try
            {
                var conversation = await _unitOfWork.ConversationRepo.GetByIdAsync(conversationId);
                
                if (conversation == null)
                {
                    _logger.LogWarning("Conversation not found for deletion: {ConversationId}", conversationId);
                    return false;
                }
                
                string userId = conversation.User_Id;
                _logger.LogInformation("Found conversation belonging to user: {UserId}", userId);
                
                await _unitOfWork.ConversationRepo.SoftDeleteAsync(conversationId);

                _logger.LogInformation("Marked conversation {ConversationId} as deleted", conversationId);
                
                // Mark as deleted if possible
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Saved changes to database for conversation deletion: {ConversationId}", conversationId);
                
                // Invalidate relevant caches
                _logger.LogInformation("Beginning cache invalidation for deleted conversation");
                
                // Invalidate conversation info cache
                var infoKey = string.Format(CacheKeys.ConversationInfo, conversationId);
                await _cacheManager.InvalidateCache(infoKey);
                _logger.LogInformation("Invalidated conversation info cache: {CacheKey}", infoKey);
                
                // Invalidate conversation messages cache
                var messagesKey = string.Format(CacheKeys.ConversationMessages, conversationId);
                await _cacheManager.InvalidateCache(messagesKey);
                _logger.LogInformation("Invalidated conversation messages cache: {CacheKey}", messagesKey);
                
                // Invalidate the user's conversation list cache
                var listKey = string.Format(CacheKeys.ConversationList, userId);
                await _cacheManager.InvalidateCache(listKey);
                _logger.LogInformation("Invalidated user conversation list cache: {CacheKey}", listKey);
                
                // Invalidate paginated and search caches
                var paginatedKey = string.Format(CacheKeys.ConversationListPaginated, userId, "*", "*");
                await _cacheManager.InvalidateCache(paginatedKey);
                _logger.LogInformation("Invalidated paginated conversation cache: {CacheKey}", paginatedKey);
                
                var searchKey = string.Format(CacheKeys.ConversationSearch, userId, "*", "*", "*");
                await _cacheManager.InvalidateCache(searchKey);
                _logger.LogInformation("Invalidated search conversation cache: {CacheKey}", searchKey);

                _logger.LogInformation("Cache invalidation completed for user {UserId}, conversation {ConversationId}", 
                    userId, conversationId);

                stopwatch.Stop();
                _logger.LogInformation("DeleteConversationAsync completed in {ElapsedMilliseconds}ms. User: {UserId}, ConversationId: {ConversationId}", 
                    stopwatch.ElapsedMilliseconds, userId, conversationId);
                
                return true;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error deleting conversation: {ConversationId} after {ElapsedMilliseconds}ms", 
                    conversationId, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<string> GenerateTitleAsync(GenerateTitleRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(request.Prompt) || string.IsNullOrEmpty(request.Response))
                throw new ArgumentException("Both prompt and response must be provided");

            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Generating title for conversation");

            try
            {
                // Create a chat history with the system message and the first interaction
                var chatHistory = new ChatHistory();
                chatHistory.AddSystemMessage("You are a helpful assistant that generates concise conversation titles.");
                chatHistory.AddUserMessage($"User: {request.Prompt}\nAssistant: {request.Response}\n\nGenerate a conversation title based on these first interaction between a user and an ai assistant. The title should be only in a format maximum three or four words");

                // Create a prompt request for the title generation
                var promptRequest = new PromptRequest
                {
                    Content = "Generate a title",
                    Model = "llama3.2:1b", // Using llama2 as the default model
                    ConversationId = Guid.NewGuid().ToString() // Temporary ID since this is a one-off request
                };

                // Get the response from Ollama
                var ollamaResponses = await _ollamaConnector.GetChatMessageContentsAsync(chatHistory, promptRequest);

                if (ollamaResponses == null || ollamaResponses.Count == 0 || string.IsNullOrEmpty(ollamaResponses[0].Content))
                {
                    _logger.LogWarning("Failed to generate title - empty response from model");
                    return "New Conversation"; // Fallback title
                }

                // Clean up the response to ensure it's just the title
                var title = ollamaResponses[0].Content.Trim();
                if (title.Length > 100) // Ensure title isn't too long
                {
                    title = title.Substring(0, 97) + "...";
                }

                stopwatch.Stop();
                _logger.LogInformation("Title generated successfully in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);

                return title;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error generating title after {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
                return "New Conversation"; // Fallback title in case of error
            }
        }
    }
}
