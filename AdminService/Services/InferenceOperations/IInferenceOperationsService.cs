using AdminService.Services.InferenceOperations.DTOs;
using OllamaSharp.Models;
using Model = OllamaSharp.Models.Model;

namespace AdminService.Services.InferenceOperations
{
    public interface IInferenceOperationsService
    {
        /// <summary>
        /// Get information about a specific model from the inference engine
        /// </summary>
        Task<ShowModelResponse?> ModelInfoAsync(string modelName);

        /// <summary>
        /// Get a paginated list of models installed on the inference engine
        /// </summary>
        Task<IEnumerable<Model>> InstalledModelsAsync(int pageNumber, int pageSize);
        
        /// <summary>
        /// Install/pull a model to the inference engine
        /// </summary>
        //Task<InstallProgressInfo> InstallModelAsync(string modelName, IProgress<InstallProgressInfo>? progress = null);
        
        /// <summary>
        /// Uninstall/remove a model from the inference engine
        /// </summary>
        Task<string> UninstallModelAsync(RemoveModelRequest model);
    }
} 