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
                CreatedAt = user.CreatedAt
            };
        }
        
        /// <summary>
        /// Maps a collection of ApplicationUser entities to UserResponse DTOs
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
