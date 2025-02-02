using Admin_Component.Connector;
using System.Globalization;
using Admin_Component.Models;
namespace Admin_Component.Services
{
    public class AdminService : IAdminService
    {
        public IOllamaAdminConnector _OllamaConnector { get; set; }
        public AdminService(IOllamaAdminConnector connector)
        {
            _OllamaConnector = connector;
        }


        public async Task<InstalledModelsResponse> InstalledModelsAsync()
        {
            var models = await _OllamaConnector.GetInstalledModelsAsync();

            return models;
        }
    }
}
