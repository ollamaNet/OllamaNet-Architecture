using Ollama_Component.Services.AuthService.Models;

namespace Ollama_Component.Services.AuthService
{
    public interface IAuthService
    {
        Task<AuthModel> RegisterUserAsync(RegisterModel model);
        Task<AuthModel> LoginUserAsync(TokenRequestModel model);
        Task<string> UpdateProfileAsync(UpdateProfileModel model);
        Task<string> ChangePasswordAsync(ChangePasswordModel model);
        Task<ForgotPasswordModel> ForgotPasswordAsync(string email);
        Task<string> ResetPasswordAsync(ResetPasswordModel model);
         Task<string> AssignRoleAsync(RoleModel model);
        Task<string> DeassignRoleAsync(RoleModel model);
    }
}
