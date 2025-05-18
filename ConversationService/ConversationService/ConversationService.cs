using ConversationService.Cache;
using ConversationService.ConversationService.DTOs;
using ConversationService.Mappers;
using Microsoft.Extensions.Logging;
using Ollama_DB_layer.DataBaseHelpers;
using Ollama_DB_layer.DTOs;
using Ollama_DB_layer.Entities;
using Ollama_DB_layer.UOW;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
                
            if (string.IsNullOrEmpty(request.FolderId))
                throw new ArgumentException("Folder ID cannot be null or empty", nameof(request.FolderId));
            
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Creating new conversation for user: {UserId}, folder: {FolderId}, model: {Model}", 
                userId, request.FolderId, request.ModelName);
                
            try
            {
                // Verify the folder belongs to the user
                var folder = await _unitOfWork.FolderRepo.GetByIdAsync(request.FolderId);
                if (folder == null || folder.User_Id != userId)
                {
                    _logger.LogWarning("Folder {FolderId} not found or does not belong to user {UserId}", 
                        request.FolderId, userId);
                    throw new InvalidOperationException("Folder not found or does not belong to user");
                }

                var conversation = new Conversation
                {
                    Title = request.Title,
                    Folder_Id = request.FolderId,
                    AI_Id = request.ModelName,
                    SystemMessage = request.SystemMessage,
                    CreatedAt = DateTime.UtcNow,
                    EndedAt = DateTime.UtcNow
                };
                
                _unitOfWork.ConversationRepo.AddAsync(conversation);
                await _unitOfWork.SaveChangesAsync();

                var conv = await _unitOfWork.ConversationRepo.GetByIdAsync(conversation.Id);
                
                if (conv == null)
                {
                    _logger.LogError("Failed to retrieve newly created conversation for user: {UserId}", userId);
                    throw new InvalidOperationException("Failed to retrieve newly created conversation");
                }
                
                _logger.LogInformation("Created conversation: {ConversationId} in folder: {FolderId} for user: {UserId}", 
                    conv.Id, conv.Folder_Id, userId);
                
                // Invalidate the folder's conversation list cache
                var cacheKey = string.Format(CacheKeys.ConversationList, conv.Folder_Id);
                await _cacheManager.InvalidateCache(cacheKey);
                
                var paginatedCacheKey = string.Format(CacheKeys.ConversationListPaginated, conv.Folder_Id, "*", "*");
                await _cacheManager.InvalidateCache(paginatedCacheKey);

                stopwatch.Stop();
                _logger.LogInformation("CreateConversationAsync completed in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
                
                return new OpenConversationResponse
                {
                    ConversationId = conv.Id,
                    FolderId = conv.Folder_Id,
                    Title = conv.Title,
                    Modelname = conv.AI_Id
                };
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
            
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Getting conversations for user: {UserId}", userId);
            
            try
            {
                // Get all folders with their conversations for the user
                var foldersWithConversations = await _unitOfWork.FolderRepo.GetAllFoldersWithConversationsAsync(userId);
                
                // Flatten the conversations from all folders
                var allConversations = foldersWithConversations
                    .SelectMany(f => f.Conversations)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToList();

                // Create a paged result manually since we're working with in-memory data
                var totalCount = allConversations.Count;
                var items = allConversations
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var result = new PagedResult<ConversationCard>
                {
                    Items = items,
                    TotalRecords = totalCount,
                    CurrentPage = pageNumber,
                    PageSize = pageSize
                };
                
                stopwatch.Stop();
                _logger.LogInformation("GetConversationsAsync completed in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
                
                return result;
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
                        
                        return new GetConversationInfoResponse
                        {
                            ConversationId = conversation.Id,
                            Title = conversation.Title,
                            ModelName = conversation.AI_Id,
                            SystemMessage = conversation.SystemMessage,
                            CreatedAt = conversation.CreatedAt,
                            TokenUsage = conversation.TokensUsed.ToString()
                        };
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

                conversation.EndedAt = DateTime.UtcNow;
                await _unitOfWork.ConversationRepo.UpdateAsync(conversation);
                await _unitOfWork.SaveChangesAsync();
                
                // Invalidate relevant caches
                var infoKey = string.Format(CacheKeys.ConversationInfo, conversationId);
                await _cacheManager.InvalidateCache(infoKey);
                
                // Invalidate the folder's conversation list cache
                var listKey = string.Format(CacheKeys.ConversationList, conversation.Folder_Id);
                await _cacheManager.InvalidateCache(listKey);
                
                // Invalidate paginated and search caches
                var paginatedKey = string.Format(CacheKeys.ConversationListPaginated, conversation.Folder_Id, "*", "*");
                await _cacheManager.InvalidateCache(paginatedKey);
                
                var searchKey = string.Format(CacheKeys.ConversationSearch, conversation.Folder_Id, "*", "*", "*");
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
                
                string folderId = conversation.Folder_Id;
                _logger.LogInformation("Found conversation in folder: {FolderId}", folderId);
                
                await _unitOfWork.ConversationRepo.SoftDeleteAsync(conversationId);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Marked conversation {ConversationId} as deleted", conversationId);
                
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
                
                // Invalidate the folder's conversation list cache
                var listKey = string.Format(CacheKeys.ConversationList, folderId);
                await _cacheManager.InvalidateCache(listKey);
                _logger.LogInformation("Invalidated folder conversation list cache: {CacheKey}", listKey);
                
                // Invalidate paginated and search caches
                var paginatedKey = string.Format(CacheKeys.ConversationListPaginated, folderId, "*", "*");
                await _cacheManager.InvalidateCache(paginatedKey);
                _logger.LogInformation("Invalidated paginated conversation cache: {CacheKey}", paginatedKey);
                
                var searchKey = string.Format(CacheKeys.ConversationSearch, folderId, "*", "*", "*");
                await _cacheManager.InvalidateCache(searchKey);
                _logger.LogInformation("Invalidated search cache: {CacheKey}", searchKey);

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

        public async Task<bool> MoveConversationToFolderAsync(string conversationId, string targetFolderId)
        {
            try
            {
                // Get the conversation
                var conversation = await _unitOfWork.ConversationRepo.GetByIdAsync(conversationId);
                if (conversation == null)
                {
                    throw new KeyNotFoundException($"Conversation with ID {conversationId} not found");
                }

                // Get the current folder to check user ownership
                var currentFolder = await _unitOfWork.FolderRepo.GetByIdAsync(conversation.Folder_Id);
                if (currentFolder == null)
                {
                    throw new KeyNotFoundException($"Current folder with ID {conversation.Folder_Id} not found");
                }

                // Get the target folder to ensure it exists
                var targetFolder = await _unitOfWork.FolderRepo.GetByIdAsync(targetFolderId);
                if (targetFolder == null)
                {
                    throw new KeyNotFoundException($"Target folder with ID {targetFolderId} not found");
                }

                // Ensure both folders belong to the same user
                if (currentFolder.User_Id != targetFolder.User_Id)
                {
                    throw new UnauthorizedAccessException("Cannot move conversation to a folder owned by a different user");
                }

                // Update the conversation's folder ID
                conversation.Folder_Id = targetFolderId;

                // Update the conversation
                await _unitOfWork.ConversationRepo.UpdateAsync(conversation);

                // Save changes
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving conversation {ConversationId} to folder {FolderId}",
                    conversationId, targetFolderId);
                throw;
            }
        }
    }
}
