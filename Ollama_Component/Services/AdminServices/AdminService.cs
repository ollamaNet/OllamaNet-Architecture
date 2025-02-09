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
        public IAIModelRepository AIModelRepo;
        public AdminService(IOllamaConnector connector, IAIModelRepository modelRepo)
        {
            _ollamaConnector = connector;
            AIModelRepo = modelRepo;
        }

        public async Task<IEnumerable<Model>> InstalledModelsAsync()
        {
            var models = await _ollamaConnector.GetInstalledModels()
                         ?? throw new InvalidOperationException("Failed to retrieve installed models.");

            return models.Any() ? models : throw new InvalidOperationException("No models are installed.");
        }


        public async Task<ShowModelResponse?> ModelInfoAsync(string modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("Model name cannot be empty.", nameof(modelName));

            return await _ollamaConnector.GetModelInfo(modelName)
                   ?? throw new InvalidOperationException($"Model '{modelName}' not found.");
        }


        public async Task<AIModel?> AddModelAsync(AddModelRequest model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Name))
                throw new ArgumentException("Invalid model data. Model name is required.");

            AIModel? dbModel = null;

            if (!model.FromOllama)
            {
                dbModel = AIModelMapper.FromRequestToAIModel(model);
            }
            else
            {
                var ollamaModelInfo = await ModelInfoAsync(model.Name);
                if (ollamaModelInfo == null)
                    throw new InvalidOperationException("Model not installed in Ollama.");

                dbModel = AIModelMapper.FromOllamaToAIModel(model, ollamaModelInfo);
            }
            
            if (dbModel == null)
                throw new InvalidOperationException("Failed to create AI model.");

            await AIModelRepo.AddAsync(dbModel);
            await AIModelRepo.SaveChangesAsync();

            return await AIModelRepo.GetByIdAsync(model.Name);
        }


        public async Task<string> UninstllModelAsync(RemoveModelRequest model)
        {
            if (model is null || string.IsNullOrWhiteSpace(model.ModelName))
                throw new ArgumentException("Invalid model request.", nameof(model));

            await _ollamaConnector.RemoveModel(model.ModelName);

            if (!model.DeleteFromDB)
                return "Model removed from Ollama";

            await AIModelRepo.SoftDeleteAsync(model.ModelName);
            await AIModelRepo.SaveChangesAsync();

            return await AIModelRepo.GetByIdAsync(model.ModelName) == null
                ? "Model removed from Ollama and DB"
                : "Model removed from Ollama but not from DB";
        }


        public async Task<string> SoftDeleteModelAsync(string modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("Model name is required.", nameof(modelName));

            var model = await AIModelRepo.GetByIdAsync(modelName);
            if (model == null)
                return "Model not found";

            await AIModelRepo.SoftDeleteAsync(modelName);
            await AIModelRepo.SaveChangesAsync();

            return "Model soft deleted successfully";
        }





    }

}
