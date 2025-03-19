using System.ComponentModel.DataAnnotations;

namespace Ollama_Component.Services.AuthService.DTOs
{
    public class TokenRequestModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
