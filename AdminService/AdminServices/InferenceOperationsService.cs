using AdminService.Connectors;
using AdminService.DTOs;
using OllamaSharp.Models;
using Model = OllamaSharp.Models.Model;

namespace AdminService
{
    public class InferenceOperationsService : IInferenceOperationsService
    {
        private readonly IOllamaConnector _ollamaConnector;

        public InferenceOperationsService(IOllamaConnector connector)
        {
            _ollamaConnector = connector;
        }






        public async Task<ShowModelResponse?> ModelInfoAsync(string modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("Model name cannot be empty.", nameof(modelName));

            return await _ollamaConnector.GetModelInfo(modelName)
                   ?? throw new InvalidOperationException($"Model '{modelName}' not found.");
        }






        public async Task<IEnumerable<Model>> InstalledModelsAsync(int pageNumber, int pageSize)
        {
            var models = await _ollamaConnector.GetInstalledModelsPaged(pageNumber, pageSize)
                         ?? throw new InvalidOperationException("Failed to retrieve installed models.");

            return models.Any() ? models : throw new InvalidOperationException("No models are installed.");
        }





        public async Task<InstallProgressInfo> InstallModelAsync(string modelName, IProgress<InstallProgressInfo>? progress = null)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("Model name cannot be empty.", nameof(modelName));

            // Check if the model is already installed
            var installedModels = await _ollamaConnector.GetInstalledModels();
            if (installedModels.Any(model => model.Name == modelName))
                return new InstallProgressInfo
                {
                    Completed = 100,
                    Total = 100,
                    Status = $"{modelName} already installed",
                    Digest = installedModels.Where(m => m.Name == modelName).FirstOrDefault()?.Digest
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




        public async Task<string> UninstallModelAsync(RemoveModelRequest model)
        {
            if (model is null || string.IsNullOrWhiteSpace(model.ModelName))
                throw new ArgumentException("Invalid model request.", nameof(model));

            await _ollamaConnector.RemoveModel(model.ModelName);
            
            return $"Model '{model.ModelName}' removed from inference engine";
        }
    }
} 