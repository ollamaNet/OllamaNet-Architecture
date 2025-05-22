using System.ComponentModel.DataAnnotations;

namespace AdminService.Services.UserOperations.DTOs
{
    /// <summary>
    /// Data transfer object for creating a new role
    /// </summary>
    public class CreateRoleRequest
    {
        /// <summary>
        /// Role name (required)
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Role description (optional)
        /// </summary>
        public string? Description { get; set; }
    }
} 