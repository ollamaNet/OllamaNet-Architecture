using System.ComponentModel.DataAnnotations;

namespace Ollama_Component.Services.AuthService.DTOs
{
    public class RoleModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string Role { get; set; }
    }
}
