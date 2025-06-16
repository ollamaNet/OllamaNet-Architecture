using System.Collections.Generic;
using System.Linq;
using AdminService.Services.UserOperations.DTOs;
using Microsoft.AspNetCore.Identity;
using Ollama_DB_layer.Entities;

namespace AdminService.Services.UserOperations.Mappers
{
    public static class UserOperationsMapper
    {
        /// <summary>
        /// Maps an ApplicationUser entity to a UserResponse DTO
        /// </summary>
        public static async Task<UserResponse> ToUserResponseAsync(ApplicationUser user, UserManager<ApplicationUser> userManager)
        {
            if (user == null) return null;
            
            // Get user roles
            var roles = await userManager.GetRolesAsync(user);
            
            return new UserResponse
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                CreatedAt = user.CreatedAt,
                Roles = roles.ToList(),
                LockoutEnd = user.LockoutEnd
            };
        }
        
        /// <summary>
        /// Synchronous version of ToUserResponse for backward compatibility
        /// </summary>
        public static UserResponse ToUserResponse(ApplicationUser user)
        {
            if (user == null) return null;
            
            return new UserResponse
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                CreatedAt = user.CreatedAt,
                // Note: Roles will be empty in this synchronous version
                Roles = new List<string>(),
                LockoutEnd = user.LockoutEnd
            };
        }
        
        /// <summary>
        /// Maps a collection of ApplicationUser entities to UserResponse DTOs asynchronously
        /// </summary>
        public static async Task<IEnumerable<UserResponse>> ToUsersListResponseAsync(IEnumerable<ApplicationUser> users, UserManager<ApplicationUser> userManager)
        {
            if (users == null) return Enumerable.Empty<UserResponse>();
            
            var userResponses = new List<UserResponse>();
            foreach (var user in users)
            {
                userResponses.Add(await ToUserResponseAsync(user, userManager));
            }
            
            return userResponses;
        }
        
        /// <summary>
        /// Maps a collection of ApplicationUser entities to UserResponse DTOs (synchronous version)
        /// </summary>
        public static IEnumerable<UserResponse> ToUsersListResponse(IEnumerable<ApplicationUser> users)
        {
            if (users == null) return Enumerable.Empty<UserResponse>();
            
            return users.Select(ToUserResponse).ToList();
        }

        /// <summary>
        /// Maps an IdentityRole to a RoleResponse DTO
        /// </summary>
        public static RoleResponse ToRoleResponse(IdentityRole role)
        {
            if (role == null) return null;
            
            return new RoleResponse
            {
                Id = role.Id,
                Name = role.Name,
                NormalizedName = role.NormalizedName,
                // Description is not available in IdentityRole by default
                Description = null
            };
        }
        
        /// <summary>
        /// Maps a collection of IdentityRole objects to RoleResponse DTOs
        /// </summary>
        public static IEnumerable<RoleResponse> ToRolesListResponse(IEnumerable<IdentityRole> roles)
        {
            if (roles == null) return Enumerable.Empty<RoleResponse>();
            
            return roles.Select(ToRoleResponse).ToList();
        }
    }
}
