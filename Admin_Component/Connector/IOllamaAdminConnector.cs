using Admin_Component.Models;


namespace Admin_Component.Connector
{
    public interface IOllamaAdminConnector
    {
        public Task<InstalledModelsResponse> GetInstalledModelsAsync();
    }
}