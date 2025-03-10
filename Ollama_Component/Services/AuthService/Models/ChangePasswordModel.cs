using System.ComponentModel.DataAnnotations;

namespace Ollama_Component.Services.AuthService.Models
{
    public class ChangePasswordModel
    {
        [Required]
        public string UserId { get; set; }

        [Required, DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required, DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }
}
