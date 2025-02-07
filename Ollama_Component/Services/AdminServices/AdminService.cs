using Ollama_Component.Connectors;
using System.Globalization;
using OllamaSharp.Models;
using Ollama_DB_layer.Repositories.AIModelRepo;
using Ollama_DB_layer.Entities;
using Ollama_Component.Services.AdminServices.Models;
using Model = OllamaSharp.Models.Model;
using Ollama_Component.Mappers;


namespace Ollama_Component.Services.AdminServices
{
    public class AdminService : IAdminService
    {
        public IOllamaConnector _ollamaConnector;
        public IAIModelRepository _aIModelRepo;
        public AdminService(IOllamaConnector connector, IAIModelRepository modelRepo)
        {
            _ollamaConnector = connector;
            _aIModelRepo = modelRepo;
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


        public async Task<AIModel> AddModelAsync(AddModelRequest model)
        {
            if (!model.FromOllama)
            {
                var DBmodel = AIModelMapper.FromRequestToAIModel(model);
                await _aIModelRepo.AddAsync(DBmodel);
                await _aIModelRepo.SaveChangesAsync();
            }
            else
            {
                var OllamaModelInfo = await GetModelInfo(model.Name);
                var DBmodel = AIModelMapper.FromOllamaToAIModel(model,OllamaModelInfo);
                await _aIModelRepo.AddAsync(DBmodel);
                await _aIModelRepo.SaveChangesAsync();
            }

            var response = await _aIModelRepo.GetByIdAsync(model.Name);

            return response;
        }
    }
}
