using ConversationService.Cache;
using ConversationService.ConversationService.DTOs;
using ConversationService.Mappers;
using Microsoft.Extensions.Logging;
using Ollama_DB_layer.DataBaseHelpers;
using Ollama_DB_layer.Entities;
using Ollama_DB_layer.UOW;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ConversationService.ConversationService
{
    public class ConversationService : IConversationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheManager _cacheManager;
        private readonly ILogger<ConversationService> _logger;

        public ConversationService(
            IUnitOfWork unitOfWork,
            ICacheManager cacheManager,
            ILogger<ConversationService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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










        public async Task<PagedResult<Conversation>> GetConversationsAsync(string userId, int pageNumber = 1, int pageSize = 15)
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





        public async Task<PagedResult<Conversation>> SearchConversationsAsync(string userId, string searchTerm, int pageNumber = 1, int pageSize = 15)
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
                
                await _unitOfWork.ConversationRepo.SoftDeleteAsync(conversationId);

                // Note: Assuming there's no direct delete method, we may need to implement
                // specific delete logic in the repository later
                _logger.LogWarning("Using SaveChangesAsync for deletion as direct Delete method is not available");
                
                // Mark as deleted if possible
                await _unitOfWork.SaveChangesAsync();
                
                // Invalidate relevant caches
                var infoKey = string.Format(CacheKeys.ConversationInfo, conversationId);
                await _cacheManager.InvalidateCache(infoKey);
                
                var messagesKey = string.Format(CacheKeys.ConversationMessages, conversationId);
                await _cacheManager.InvalidateCache(messagesKey);
                
                // Invalidate the user's conversation list cache
                var listKey = string.Format(CacheKeys.ConversationList, userId);
                await _cacheManager.InvalidateCache(listKey);
                
                // Invalidate paginated and search caches
                var paginatedKey = string.Format(CacheKeys.ConversationListPaginated, userId, "*", "*");
                await _cacheManager.InvalidateCache(paginatedKey);
                
                var searchKey = string.Format(CacheKeys.ConversationSearch, userId, "*", "*", "*");
                await _cacheManager.InvalidateCache(searchKey);

                stopwatch.Stop();
                _logger.LogInformation("DeleteConversationAsync completed in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
                
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
    }
}
