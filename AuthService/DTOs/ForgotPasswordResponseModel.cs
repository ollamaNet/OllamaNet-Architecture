namespace AuthenticationService.DTOs
{
    public class ForgotPasswordResponseModel
    {
        public string Token { get; set; }
        public string ResetPasswordLink { get; set; }
    }
}
