using System.ComponentModel.DataAnnotations;

namespace Ollama_Component.Services.AuthService.Models
{
    public class ForgotPasswordRequestModel
    {
        [Required]
        public string Email { get; set; }
    }
}
