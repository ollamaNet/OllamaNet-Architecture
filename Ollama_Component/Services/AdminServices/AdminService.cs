using Ollama_Component.Connectors;
using System.Globalization;
using OllamaSharp.Models;
using Ollama_DB_layer.Repositories.AIModelRepo;
using Ollama_DB_layer.Entities;


namespace Ollama_Component.Services.AdminServices
{
    public class AdminService : IAdminService
    {
        public IOllamaConnector _ollamaConnector;
        public IAIModelRepository _aIModelRepo;
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
            var model = await _ollamaConnector.GetModelInfo(modelName);

            return model;
        }

    }
}
