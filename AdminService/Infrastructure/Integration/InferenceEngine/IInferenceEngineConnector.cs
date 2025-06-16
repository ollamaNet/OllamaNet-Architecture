using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdminService.Services.InferenceOperations.DTOs;
using OllamaSharp;
using OllamaSharp.Models;
using Model = OllamaSharp.Models.Model;

namespace AdminService.Infrastructure.Integration.InferenceEngine
{
    /// <summary>
    /// Interface for connecting to inference engines
    /// </summary>
    public interface IInferenceEngineConnector
    {
        /// <summary>
        /// Gets connector attributes
        /// </summary>
        IReadOnlyDictionary<string, object?> Attributes { get; }

        /// <summary>
        /// Gets all installed models
        /// </summary>
        /// <returns>Collection of models</returns>
        Task<IEnumerable<Model>> GetInstalledModels();

        /// <summary>
        /// Gets installed models with pagination
        /// </summary>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Collection of models</returns>
        Task<IEnumerable<Model>> GetInstalledModelsPaged(int pageNumber, int pageSize);

        /// <summary>
        /// Gets information about a model
        /// </summary>
        /// <param name="modelName">Name of the model</param>
        /// <returns>Model information</returns>
        Task<ShowModelResponse> GetModelInfo(string modelName);

        /// <summary>
        /// Removes a model
        /// </summary>
        /// <param name="modelName">Name of the model to remove</param>
        /// <returns>Status message</returns>
        Task<string> RemoveModel(string modelName);

        /// <summary>
        /// Pulls a model asynchronously
        /// </summary>
        /// <param name="modelName">Name of the model to pull</param>
        /// <returns>Stream of progress information</returns>
        IAsyncEnumerable<InstallProgressInfo> PullModelAsync(string modelName);

        /// <summary>
        /// Gets the base URL of the inference engine
        /// </summary>
        /// <returns>Base URL</returns>
        string GetBaseUrl();
    }
} 