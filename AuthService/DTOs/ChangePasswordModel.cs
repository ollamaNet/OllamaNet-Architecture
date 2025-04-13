using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.DTOs
{
    public class ChangePasswordModel
    {

        [Required, DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required, DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }
}
