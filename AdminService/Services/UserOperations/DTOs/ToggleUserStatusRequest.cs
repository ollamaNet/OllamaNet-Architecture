using System.ComponentModel.DataAnnotations;

namespace AdminService.Services.UserOperations.DTOs
{
    public class ToggleUserStatusRequest
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public bool IsActive { get; set; }
    }
} 