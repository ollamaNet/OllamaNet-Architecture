namespace AuthenticationService.DTOs
{
    public class ForgotPasswordResponseModel
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string Token { get; set; } // Kept for internal use or debugging
    }
}
