using Ollama_Component.Connectors;
using Ollama_Component.Services.AdminServices.Models;
using System.Globalization;

namespace Ollama_Component.Services
{
    public class AdminService : IAdminService
    {
        public IOllamaConnector _OllamaConnector;
        public AdminService(IOllamaConnector connector)
        {
            _OllamaConnector = connector;
        }


        public async Task<string> InstalledModelsAsync()
        {
            var models = "installed models";

            return models;
        }

    }
}
