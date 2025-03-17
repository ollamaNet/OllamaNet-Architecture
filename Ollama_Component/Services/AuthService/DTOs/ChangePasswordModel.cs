using System.ComponentModel.DataAnnotations;

namespace Ollama_Component.Services.AuthService.DTOs
{
    public class ChangePasswordModel
    {

        [Required, DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required, DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }
}
