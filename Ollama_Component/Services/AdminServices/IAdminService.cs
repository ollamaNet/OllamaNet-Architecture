using Admin_Component.Connector;
using Admin_Component.Models;

namespace Admin_Component.Services
{
    public interface IAdminService
    {
        IOllamaAdminConnector _OllamaConnector { get; set; }

        Task<InstalledModelsResponse> InstalledModelsAsync();
    }
}