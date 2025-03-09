using Ollama_Component.Services.AuthService.Models;

namespace Ollama_Component.Services.AuthService
{
    public interface IAuthService
    {
        Task<AuthModel> RegisterUserAsync(RegisterModel model);
        Task<AuthModel> LoginUserAsync(TokenRequestModel model);
        Task<string> AssignRoleAsync(AddRoleModel model);
    }
}
