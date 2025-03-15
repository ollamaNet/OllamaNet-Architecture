using System.ComponentModel.DataAnnotations;

namespace Ollama_Component.Services.AuthService.Models
{
    public class UpdateProfileModel
    {

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Username { get; set; }
    }
}
