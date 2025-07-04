﻿using ConversationServices.Services.FolderService.DTOs;
using Ollama_DB_layer.DTOs;

namespace ConversationServices.Services.FolderService

{
    public interface IFolderService
    {
        Task<FolderWithConversations> CreateFolderAsync(string userId, CreateFolderRequest request);
        Task<bool> DeleteFolderAsync(string folderId);
        Task<bool> SoftDeleteFolderAsync(string folderId);
        Task<bool> UpdateFolderAsync(string userId, UpdateFolderRequest request);
        Task<IEnumerable<FolderWithConversations>> GetFoldersByUserIdAsync(string userId);
        Task<FolderWithConversations> GetAllMainAsync(string userId, string rootFolderId);
        Task<FolderWithConversations> GetRootFolderStructureAsync(string userId);
    }
}
