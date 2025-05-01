using AuthenticationService.DTOs;
using Ollama_DB_layer.Entities;

namespace AuthenticationService
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
        Task<AuthModel> RefreshTokenAsync(string refreshtoken);
        Task<bool> LoggoutAsync(string refreshtoken);
        Task<List<string>> GetRolesAsync(string userId);

    }
}
