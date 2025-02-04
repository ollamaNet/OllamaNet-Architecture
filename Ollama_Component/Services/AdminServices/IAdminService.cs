using Ollama_Component.Services.AdminServices.Models;

namespace Ollama_Component.Services
{
    public interface IAdminService
    {
        Task<string> InstalledModelsAsync();
    }
}