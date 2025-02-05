using Ollama_Component.Connectors;
using System.Globalization;
using OllamaSharp.Models;


namespace Ollama_Component.Services.AdminServices
{
    public class AdminService : IAdminService
    {
        public IOllamaConnector _ollamaConnector;
        public AdminService(IOllamaConnector connector)
        {
            _ollamaConnector = connector;
        }


        public async Task<IEnumerable<Model>> InstalledModelsAsync()
        {
            var models = await _ollamaConnector.GetInstalledModels();

            return models;
        }


        public async Task<ShowModelResponse> GetModelInfo(string modelName)
        {
            var models = await _ollamaConnector.GetModelInfo(modelName);
            return models;
        }


    }
}
