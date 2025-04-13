using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.DTOs
{
    public class UpdateProfileModel
    {

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Username { get; set; }
    }
}
