using Ollama_Component.Services.AuthService.Models;
using OllamaSharp.Models;

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
        Task<AuthModel> RefreshTokenAsync(string refreshtoken);
        Task<bool> LoggoutAsync(string refreshtoken);
        Task<List<string>> GetRolesAsync(string userId);

    }
}
