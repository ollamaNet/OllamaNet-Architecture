namespace Ollama_Component.Services.AuthService.Models
{
    public class ForgotPasswordResponseModel
    {
        public string Token { get; set; }
        public string ResetPasswordLink { get; set; }
    }
}
