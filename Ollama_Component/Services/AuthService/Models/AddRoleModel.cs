using System.ComponentModel.DataAnnotations;

namespace Ollama_Component.Services.AuthService.Models
{
    public class AddRoleModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string Role { get; set; }
    }
}
