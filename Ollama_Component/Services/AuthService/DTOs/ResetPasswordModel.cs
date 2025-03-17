using System.ComponentModel.DataAnnotations;

namespace Ollama_Component.Services.AuthService.Models
{
    public class ResetPasswordModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }

        [Required, DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }
}
