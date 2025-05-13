using System.ComponentModel.DataAnnotations;

namespace AdminService.Services.UserOperations.DTOs
{
    public class CreateUserRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "User"; // Default role
    }
} 