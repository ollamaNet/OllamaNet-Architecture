using AdminService.Services.UserOperations.DTOs;

namespace AdminService.Services.UserOperations
{
    public interface IUserOperationsService
    {
        // Read operations
        Task<UserResponse> GetUserByIdAsync(string userId);
        Task<UserResponse> GetUserByEmailAsync(string email);
        Task<IEnumerable<UserResponse>> SearchUsersAsync(string searchTerm, string? role = null);
        Task<(IEnumerable<UserResponse> Users, int TotalCount)> GetUsersPaginatedAsync(int pageNumber, int pageSize, string? role = null);
        
        // Update operations
        Task<bool> ChangeUserRoleAsync(string userId, string newRole);
        Task<bool> ToggleUserStatusAsync(string userId, bool isActive);
        
        // Delete operations
        Task<bool> DeleteUserAsync(string userId);
        Task<bool> SoftDeleteUserAsync(string userId);
        
        // Security operations
        Task<bool> LockUserAccountAsync(string userId, TimeSpan duration);
        Task<bool> UnlockUserAccountAsync(string userId);
    }
}