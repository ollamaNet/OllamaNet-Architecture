using Ollama_Component.Connectors;
using System.Globalization;
using OllamaSharp.Models;
using Ollama_DB_layer.Repositories.AIModelRepo;
using Ollama_DB_layer.Entities;
using Ollama_Component.Services.AdminServices.Models;
using Model = OllamaSharp.Models.Model;
using OllamaSharp;
using Ollama_Component.Mappers.DbMappers;


namespace Ollama_Component.Services.AdminServices
{
    public class AdminService : IAdminService
    {
        public readonly IOllamaConnector _ollamaConnector;
        public readonly IAIModelRepository _aiModelRepo;
        public AdminService(IOllamaConnector connector, IAIModelRepository modelRepo)
        {
            _ollamaConnector = connector;
            _aiModelRepo = modelRepo;
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
                dbModel = AIModelMapper.FromRequestToAIModel(model);
            else
            {
                var ollamaModelInfo = await ModelInfoAsync(model.Name);
                if (ollamaModelInfo == null)
                    throw new InvalidOperationException("Model not installed in Ollama.");

                dbModel = AIModelMapper.FromOllamaToAIModel(model, ollamaModelInfo);
            }

            if (dbModel == null)
                throw new InvalidOperationException("Failed to create AI model.");

            await _aiModelRepo.AddAsync(dbModel);
            await _aiModelRepo.SaveChangesAsync();

            return await _aiModelRepo.GetByIdAsync(model.Name);
        }


        public async Task<string> UninstllModelAsync(RemoveModelRequest model)
        {
            if (model is null || string.IsNullOrWhiteSpace(model.ModelName))
                throw new ArgumentException("Invalid model request.", nameof(model));

            await _ollamaConnector.RemoveModel(model.ModelName);

            if (!model.DeleteFromDB)
            {
                await SoftDeleteAIModelAsync(model.ModelName);
                return "Model removed from Ollama";
            }
               
            return await _aiModelRepo.GetByIdAsync(model.ModelName) == null
                ? "Model removed from Ollama and DB"
                : "Model removed from Ollama but not from DB";
        }


        public async Task<string> SoftDeleteAIModelAsync(string modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("Model name is required.", nameof(modelName));

            var model = await _aiModelRepo.GetByIdAsync(modelName);
            if (model == null)
                return "Model not found";

            await _aiModelRepo.SoftDeleteAsync(modelName);
            await _aiModelRepo.SaveChangesAsync();

            return "Model soft deleted successfully";
        }


        public async Task<InstallProgressInfo> InstallModelAsync(string modelName, IProgress<InstallProgressInfo>? progress = null)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("Model name cannot be empty.", nameof(modelName));

            // Check if the model is already installed
            var installedModels = await InstalledModelsAsync();
            if (installedModels.Any(model => model.Name == modelName))
                return new InstallProgressInfo
                {
                    Completed = 100,
                    Total = 100,
                    Status = $"{modelName}already installed",
                    Digest = installedModels.Where(installedModels => installedModels.Name == modelName).FirstOrDefault()?.Digest

                };

            else
            {
                InstallProgressInfo? lastProgress = null;

                await foreach (var response in _ollamaConnector.PullModelAsync(modelName))
                {
                    lastProgress = response; // Store the latest progress

                    // Report progress to the caller (e.g., the controller)
                    progress?.Report(response);
                }
                return lastProgress ?? throw new InvalidOperationException($"Failed to install model '{modelName}'.");
            }
        }








    }
}
