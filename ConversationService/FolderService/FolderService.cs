using ConversationService.Cache;
using ConversationService.FolderService.DTOs;
using Microsoft.Extensions.Logging;
using Ollama_DB_layer.DTOs;
using Ollama_DB_layer.Entities;
using Ollama_DB_layer.UOW;
using System.Diagnostics;

namespace ConversationService.FolderService
{
    public class FolderService : IFolderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheManager _cacheManager;
        private readonly ILogger<FolderService> _logger;

        public FolderService(
            IUnitOfWork unitOfWork,
            ICacheManager cacheManager,
            ILogger<FolderService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<FolderWithConversations> CreateFolderAsync(string userId, CreateFolderRequest request)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(request.Name))
                throw new ArgumentException("Folder name cannot be null or empty", nameof(request.Name));

            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Creating new folder for user: {UserId}, name: {Name}, rootFolderId: {RootFolderId}",
                userId, request.Name, request.RootFolderId);

            try
            {
                // Check if folder name is unique for the user
                if (!await _unitOfWork.FolderRepo.IsFolderNameUniqueForUserAsync(userId, request.Name))
                {
                    _logger.LogWarning("Folder name {Name} already exists for user {UserId}", request.Name, userId);
                    throw new InvalidOperationException($"A folder with name '{request.Name}' already exists for this user.");
                }

                // If root folder ID is provided, verify it exists and belongs to the user
                if (!string.IsNullOrEmpty(request.RootFolderId))
                {
                    var rootFolder = await _unitOfWork.FolderRepo.GetByIdAsync(request.RootFolderId);
                    if (rootFolder == null || rootFolder.User_Id != userId)
                    {
                        _logger.LogWarning("Root folder {RootFolderId} not found or does not belong to user {UserId}",
                            request.RootFolderId, userId);
                        throw new InvalidOperationException("Root folder not found or does not belong to user");
                    }
                }

                var folder = new Folder
                {
                    Name = request.Name,
                    User_Id = userId,
                    RootFolderId = request.RootFolderId,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.FolderRepo.AddAsync(folder);
                await _unitOfWork.SaveChangesAsync();

                // Get the created folder with its conversations
                var createdFolder = await _unitOfWork.FolderRepo.GetFolderWithConversationsByIdAsync(folder.Id);

                stopwatch.Stop();
                _logger.LogInformation("CreateFolderAsync completed in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);

                return createdFolder;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error creating folder for user: {UserId} after {ElapsedMilliseconds}ms",
                    userId, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<bool> DeleteFolderAsync(string folderId)
        {
            if (string.IsNullOrEmpty(folderId))
                throw new ArgumentException("Folder ID cannot be null or empty", nameof(folderId));

            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Deleting folder: {FolderId}", folderId);

            try
            {
                var folder = await _unitOfWork.FolderRepo.GetByIdAsync(folderId);
                if (folder == null)
                {
                    _logger.LogWarning("Folder not found for deletion: {FolderId}", folderId);
                    return false;
                }

                await _unitOfWork.FolderRepo.DeleteAsync(folderId);
                await _unitOfWork.SaveChangesAsync();

                stopwatch.Stop();
                _logger.LogInformation("DeleteFolderAsync completed in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);

                return true;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error deleting folder: {FolderId} after {ElapsedMilliseconds}ms",
                    folderId, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<bool> SoftDeleteFolderAsync(string folderId)
        {
            if (string.IsNullOrEmpty(folderId))
                throw new ArgumentException("Folder ID cannot be null or empty", nameof(folderId));

            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Soft deleting folder: {FolderId}", folderId);

            try
            {
                var folder = await _unitOfWork.FolderRepo.GetByIdAsync(folderId);
                if (folder == null)
                {
                    _logger.LogWarning("Folder not found for soft deletion: {FolderId}", folderId);
                    return false;
                }

                await _unitOfWork.FolderRepo.SoftDeleteAsync(folderId);
                await _unitOfWork.SaveChangesAsync();

                stopwatch.Stop();
                _logger.LogInformation("SoftDeleteFolderAsync completed in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);

                return true;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error soft deleting folder: {FolderId} after {ElapsedMilliseconds}ms",
                    folderId, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<bool> UpdateFolderAsync(string userId, UpdateFolderRequest request)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(request.FolderId))
                throw new ArgumentException("Folder ID cannot be null or empty", nameof(request.FolderId));

            if (string.IsNullOrEmpty(request.NewName))
                throw new ArgumentException("New folder name cannot be null or empty", nameof(request.NewName));

            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Updating folder: {FolderId}, new name: {NewName}", request.FolderId, request.NewName);

            try
            {
                var folder = await _unitOfWork.FolderRepo.GetByIdAsync(request.FolderId);
                if (folder == null || folder.User_Id != userId)
                {
                    _logger.LogWarning("Folder {FolderId} not found or does not belong to user {UserId}",
                        request.FolderId, userId);
                    return false;
                }

                // Check if new name is unique for the user
                if (folder.Name != request.NewName && 
                    !await _unitOfWork.FolderRepo.IsFolderNameUniqueForUserAsync(userId, request.NewName))
                {
                    _logger.LogWarning("Folder name {Name} already exists for user {UserId}", request.NewName, userId);
                    throw new InvalidOperationException($"A folder with name '{request.NewName}' already exists for this user.");
                }

                folder.Name = request.NewName;
                folder.ModifiedAt = DateTime.UtcNow;

                await _unitOfWork.FolderRepo.UpdateAsync(folder);
                await _unitOfWork.SaveChangesAsync();

                stopwatch.Stop();
                _logger.LogInformation("UpdateFolderAsync completed in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);

                return true;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error updating folder: {FolderId} after {ElapsedMilliseconds}ms",
                    request.FolderId, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<IEnumerable<FolderWithConversations>> GetFoldersByUserIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Getting folders for user: {UserId}", userId);

            try
            {
                var folders = await _unitOfWork.FolderRepo.GetAllFoldersWithConversationsAsync(userId);

                stopwatch.Stop();
                _logger.LogInformation("GetFoldersByUserIdAsync completed in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);

                return folders;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error getting folders for user: {UserId} after {ElapsedMilliseconds}ms",
                    userId, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<FolderWithConversations> GetAllMainAsync(string userId, string rootFolderId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            if (string.IsNullOrEmpty(rootFolderId))
                throw new ArgumentException("Root folder ID cannot be null or empty", nameof(rootFolderId));

            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Getting main folder structure for user: {UserId}, root folder: {RootFolderId}",
                userId, rootFolderId);

            try
            {
                var folderStructure = await _unitOfWork.FolderRepo.GetSubFoldersAndRootConversationsAsync(userId, rootFolderId);

                stopwatch.Stop();
                _logger.LogInformation("GetAllMainAsync completed in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);

                return folderStructure;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error getting main folder structure for user: {UserId} after {ElapsedMilliseconds}ms",
                    userId, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
