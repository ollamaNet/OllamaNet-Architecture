using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using AdminService.Services.UserOperations.DTOs;
using AdminService.Services.UserOperations.Exceptions;
using AdminService.Services.UserOperations.Mappers;
using Ollama_DB_layer.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Services.UserOperations
{
    public class UserOperationsService : IUserOperationsService
    {
        private readonly ILogger<UserOperationsService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserOperationsService(
            ILogger<UserOperationsService> logger,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
        }




        public async Task<UserResponse> GetUserByIdAsync(string userId)
        {
            _logger.LogInformation($"Getting user by ID {userId}");
            
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {userId} not found");
                throw new UserNotFoundException(userId);
            }
            
            return UserOperationsMapper.ToUserResponse(user);
        }




        public async Task<bool> ToggleUserStatusAsync(string userId, bool isActive)
        {
            _logger.LogInformation($"Toggling user {userId} status to {(isActive ? "active" : "inactive")}");
            
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {userId} not found");
                throw new UserNotFoundException(userId);
            }
            
            user.IsActive = isActive;
            var result = await _userManager.UpdateAsync(user);
            
            if (!result.Succeeded)
            {
                _logger.LogError($"Failed to update user status: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                throw new InvalidUserOperationException(userId, "toggle status", result.Errors.FirstOrDefault()?.Description);
            }
            
            _logger.LogInformation($"Successfully toggled user {userId} status to {(isActive ? "active" : "inactive")}");
            return true;
        }






        public async Task<bool> DeleteUserAsync(string userId)
        {
            _logger.LogInformation($"Deleting user {userId}");
            
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {userId} not found");
                throw new UserNotFoundException(userId);
            }
            
            var result = await _userManager.DeleteAsync(user);
            
            if (!result.Succeeded)
            {
                _logger.LogError($"Failed to delete user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                throw new InvalidUserOperationException(userId, "delete", result.Errors.FirstOrDefault()?.Description);
            }
            
            _logger.LogInformation($"Successfully deleted user {userId}");
            return true;
        }




        public async Task<bool> SoftDeleteUserAsync(string userId)
        {
            _logger.LogInformation($"Soft deleting user {userId}");
            
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {userId} not found");
                throw new UserNotFoundException(userId);
            }
            
            user.IsDeleted = true;
            var result = await _userManager.UpdateAsync(user);
            
            if (!result.Succeeded)
            {
                _logger.LogError($"Failed to soft delete user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                throw new InvalidUserOperationException(userId, "soft delete", result.Errors.FirstOrDefault()?.Description);
            }
            
            _logger.LogInformation($"Successfully soft deleted user {userId}");
            return true;
        }





        public async Task<bool> ChangeUserRoleAsync(string userId, string newRole)
        {
            _logger.LogInformation($"Changing user {userId} role to {newRole}");
            
            // Validate the role exists
            if (string.IsNullOrEmpty(newRole))
            {
                throw new ArgumentException("New role cannot be null or empty", nameof(newRole));
            }
            
            var roleExists = await _roleManager.RoleExistsAsync(newRole);
            if (!roleExists)
            {
                _logger.LogWarning($"Role {newRole} does not exist");
                throw new InvalidUserOperationException(userId, "change role", $"Role '{newRole}' does not exist");
            }
            
            // Get the user
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {userId} not found");
                throw new UserNotFoundException(userId);
            }

            
            // Get current roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            


            // Remove from current roles
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                _logger.LogError($"Failed to remove user from current roles: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}");
                throw new InvalidUserOperationException(userId, "change role", removeResult.Errors.FirstOrDefault()?.Description);
            }
            

            // Add to new role
            var addResult = await _userManager.AddToRoleAsync(user, newRole);
            if (!addResult.Succeeded)
            {
                _logger.LogError($"Failed to add user to new role: {string.Join(", ", addResult.Errors.Select(e => e.Description))}");
                throw new InvalidUserOperationException(userId, "change role", addResult.Errors.FirstOrDefault()?.Description);
            }
            
            _logger.LogInformation($"Successfully changed user {userId} role to {newRole}");
            return true;
        }





        public async Task<bool> LockUserAccountAsync(string userId, TimeSpan duration)
        {
            _logger.LogInformation($"Locking user {userId} for {duration.TotalMinutes} minutes");
            
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {userId} not found");
                throw new UserNotFoundException(userId);
            }
            
            // Set lockout end date
            var lockoutEnd = DateTime.UtcNow.Add(duration);
            var result = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
            
            if (!result.Succeeded)
            {
                _logger.LogError($"Failed to lock user account: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                throw new InvalidUserOperationException(userId, "lock account", result.Errors.FirstOrDefault()?.Description);
            }
            
            _logger.LogInformation($"Successfully locked user {userId} until {lockoutEnd}");
            return true;
        }





        public async Task<bool> UnlockUserAccountAsync(string userId)
        {
            _logger.LogInformation($"Unlocking user {userId}");
            
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {userId} not found");
                throw new UserNotFoundException(userId);
            }
            
            // Clear lockout end date
            var result = await _userManager.SetLockoutEndDateAsync(user, null);
            
            if (!result.Succeeded)
            {
                _logger.LogError($"Failed to unlock user account: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                throw new InvalidUserOperationException(userId, "unlock account", result.Errors.FirstOrDefault()?.Description);
            }
            
            _logger.LogInformation($"Successfully unlocked user {userId}");
            return true;
        }




        public async Task<(IEnumerable<UserResponse> Users, int TotalCount)> GetUsersPaginatedAsync(int pageNumber, int pageSize, string? role = null)
        {
            _logger.LogInformation($"Getting users page {pageNumber}, size {pageSize}, role '{role}'");
            
            IQueryable<ApplicationUser> query = _userManager.Users.Where(u => !u.IsDeleted);
            
            // Filter by role if specified
            if (!string.IsNullOrEmpty(role))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role);
                var userIds = usersInRole.Select(u => u.Id);
                query = query.Where(u => userIds.Contains(u.Id));
            }
            
            // Get total count
            int totalCount = await query.CountAsync();
            
            // Apply pagination
            var users = await query
                .OrderBy(u => u.UserName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            // Map to response DTOs using the mapper
            var userResponses = UserOperationsMapper.ToUsersListResponse(users);
            
            return (userResponses, totalCount);
        }


        public async Task<IEnumerable<UserResponse>> SearchUsersAsync(string searchTerm, string? role = null)
        {
            _logger.LogInformation($"Searching users with term '{searchTerm}' and role '{role}'");
            
            if (string.IsNullOrEmpty(searchTerm))
            {
                throw new ArgumentException("Search term cannot be null or empty", nameof(searchTerm));
            }
            
            // Create base query for active users
            IQueryable<ApplicationUser> query = _userManager.Users.Where(u => !u.IsDeleted);
            
            // Apply search term filter (case-insensitive)
            searchTerm = searchTerm.ToLower();
            query = query.Where(u =>
                u.UserName.ToLower().Contains(searchTerm) ||
                u.Email.ToLower().Contains(searchTerm));
            
            // Get users matching search criteria
            var matchingUsers = await query.ToListAsync();
            
            // Filter by role if specified
            if (!string.IsNullOrEmpty(role))
            {
                var filteredUsers = new List<ApplicationUser>();
                foreach (var user in matchingUsers)
                {
                    if (await _userManager.IsInRoleAsync(user, role))
                    {
                        filteredUsers.Add(user);
                    }
                }
                matchingUsers = filteredUsers;
            }
            
            // Map to response DTOs using the mapper
            return UserOperationsMapper.ToUsersListResponse(matchingUsers);
        }







        public async Task<IEnumerable<RoleResponse>> GetAvailableRolesAsync()
        {
            _logger.LogInformation("Getting all available roles");
            
            // Get all roles from RoleManager
            var roles = await _roleManager.Roles.ToListAsync();
            
            // Map roles to RoleResponse DTOs using the mapper
            return UserOperationsMapper.ToRolesListResponse(roles);
        }





        public async Task<RoleResponse> CreateRoleAsync(CreateRoleRequest request)
        {
            _logger.LogInformation($"Creating new role with name '{request.Name}'");
            
            // Check if role already exists
            var roleExists = await _roleManager.RoleExistsAsync(request.Name);
            if (roleExists)
            {
                _logger.LogWarning($"Role with name '{request.Name}' already exists");
                throw new InvalidOperationException($"Role with name '{request.Name}' already exists");
            }
            
            // Create new role
            var role = new IdentityRole
            {
                Name = request.Name,
                NormalizedName = request.Name.ToUpper(),
                // Store description in claims if needed
            };
            
            var result = await _roleManager.CreateAsync(role);
            
            if (!result.Succeeded)
            {
                _logger.LogError($"Failed to create role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                throw new InvalidOperationException($"Failed to create role: {result.Errors.FirstOrDefault()?.Description}");
            }
            
            // Add description claim if provided
            if (!string.IsNullOrEmpty(request.Description))
            {
                await _roleManager.AddClaimAsync(role, new System.Security.Claims.Claim("Description", request.Description));
            }
            
            _logger.LogInformation($"Successfully created role '{request.Name}'");
            
            // Return the created role
            var createdRole = await _roleManager.FindByNameAsync(request.Name);
            return UserOperationsMapper.ToRoleResponse(createdRole);
        }
        


        public async Task<bool> DeleteRoleAsync(string roleId)
        {
            _logger.LogInformation($"Deleting role with ID '{roleId}'");
            
            // Find the role
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                _logger.LogWarning($"Role with ID '{roleId}' not found");
                throw new KeyNotFoundException($"Role with ID '{roleId}' not found");
            }
            
            // Check if role is in use
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
            if (usersInRole.Any())
            {
                _logger.LogWarning($"Cannot delete role '{role.Name}' because it is assigned to {usersInRole.Count} user(s)");
                throw new InvalidOperationException($"Cannot delete role '{role.Name}' because it is assigned to {usersInRole.Count} user(s)");
            }
            
            // Delete the role
            var result = await _roleManager.DeleteAsync(role);
            
            if (!result.Succeeded)
            {
                _logger.LogError($"Failed to delete role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                throw new InvalidOperationException($"Failed to delete role: {result.Errors.FirstOrDefault()?.Description}");
            }
            
            _logger.LogInformation($"Successfully deleted role '{role.Name}'");
            return true;
        }
    }
}