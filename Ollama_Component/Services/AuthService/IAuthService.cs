using Ollama_Component.Services.AuthService.Models;
using Ollama_DB_layer.Entities;

namespace Ollama_Component.Services.AuthService
{
    public interface IAuthService
    {
        Task<ApplicationUser> GetUserByTokenAsync(string token);
        Task<AuthModel> RegisterUserAsync(RegisterModel model);
        Task<AuthModel> LoginUserAsync(TokenRequestModel model);
        Task<string> UpdateProfileAsync(UpdateProfileModel model,string token);
        Task<string> ChangePasswordAsync(ChangePasswordModel model, string token);
        Task<ForgotPasswordResponseModel> ForgotPasswordAsync(ForgotPasswordRequestModel model);
        Task<string> ResetPasswordAsync(ResetPasswordModel model);
        Task<string> AssignRoleAsync(RoleModel model);
        Task<string> DeassignRoleAsync(RoleModel model);
    }
}
