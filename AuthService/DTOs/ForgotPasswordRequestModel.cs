using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.DTOs
{
    public class ForgotPasswordRequestModel
    {
        [Required]
        public string Email { get; set; }
    }
}
