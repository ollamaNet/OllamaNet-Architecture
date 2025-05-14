using System.ComponentModel.DataAnnotations;

namespace AdminService.Services.UserOperations.DTOs
{
    public class ChangeUserRoleRequest
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string NewRole { get; set; } = string.Empty;
    }
} 