using System.ComponentModel.DataAnnotations;

namespace AdminService.Services.UserOperations.DTOs
{
    public class LockUserRequest
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int LockoutMinutes { get; set; } = 30; // Default 30 minutes
    }
} 