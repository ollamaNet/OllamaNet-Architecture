using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OllamaSharp;
using OllamaSharp.Models;
using AdminService.Infrastructure.Configuration;
using Model = OllamaSharp.Models.Model;
using AdminService.Services.InferenceOperations.DTOs;

namespace AdminService.Infrastructure.Integration.InferenceEngine
{
    /// <summary>
    /// Connector for inference engines that implements both the new IInferenceEngineConnector
    /// and the legacy IOllamaConnector for backward compatibility during transition
    /// </summary>
    public class InferenceEngineConnector : IInferenceEngineConnector
    {
        private readonly IInferenceEngineConfiguration _configuration;
        private readonly ILogger<InferenceEngineConnector> _logger;
        private IOllamaApiClient _ollamaApiClient;
        private readonly object _lockObject = new object();

        /// <summary>
        /// Constructor for InferenceEngineConnector
        /// </summary>
        /// <param name="configuration">Inference engine configuration</param>
        /// <param name="logger">Logger</param>
        public InferenceEngineConnector(
            IInferenceEngineConfiguration configuration,
            ILogger<InferenceEngineConnector> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            // Initialize API client with the current base URL
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                var baseUrl = _configuration.GetBaseUrl();
                _logger.LogInformation("Initializing InferenceEngineConnector with URL: {Url}", baseUrl);
                _ollamaApiClient = new OllamaApiClient(baseUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing OllamaApiClient");
                // Fallback to default URL
                _ollamaApiClient = new OllamaApiClient("http://localhost:11434");
            }
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

        /// <inheritdoc />
        public async Task<IEnumerable<Model>> GetInstalledModels()
        {
            try
            {
                await EnsureClientIsInitialized();
                
                lock (_lockObject)
                {
                    _logger.LogDebug("Getting installed models from {Url}", GetBaseUrl());

                    var response = _ollamaApiClient.ListLocalModelsAsync().GetAwaiter().GetResult();
                    
                    return response;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting installed models");
                return Enumerable.Empty<Model>();
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Model>> GetInstalledModelsPaged(int pageNumber, int pageSize)
        {
            try
            {
                await EnsureClientIsInitialized();
                
                var response = await _ollamaApiClient.ListLocalModelsAsync();

                // Ensure pageNumber and pageSize are valid
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;

                // Calculate the items to skip and take for pagination
                var skip = (pageNumber - 1) * pageSize;
                var pagedResponse = response.Skip(skip).Take(pageSize);

                return pagedResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged installed models");
                return Enumerable.Empty<Model>();
            }
        }





        /// <inheritdoc />
        public async Task<ShowModelResponse> GetModelInfo(string modelName)
        {
            try
            {
                await EnsureClientIsInitialized();
                
                lock (_lockObject)
                {
                    _logger.LogDebug("Getting model info for {ModelName} from {Url}", modelName, GetBaseUrl());

                    var response = _ollamaApiClient.ShowModelAsync(modelName).GetAwaiter().GetResult();
                    
                    return response;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting model info for {ModelName}", modelName);
                throw;
            }
        }




        /// <inheritdoc />
        public async Task<string> RemoveModel(string modelName)
        {
            try
            {
                await EnsureClientIsInitialized();
                
                await _ollamaApiClient.DeleteModelAsync(modelName);

                var models = await _ollamaApiClient.ListLocalModelsAsync();
                var modelExists = models.Any(m => m.Name == modelName);

                return modelExists ? "Model not removed successfully" : "Model removed successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing model {ModelName}", modelName);
                throw;
            }
        }





        /// <inheritdoc />
        public async IAsyncEnumerable<InstallProgressInfo> PullModelAsync(string modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("Model name cannot be empty.", nameof(modelName));

            await EnsureClientIsInitialized();

            await foreach (var response in _ollamaApiClient.PullModelAsync(modelName))
            {
                yield return new InstallProgressInfo
                {
                    Status = response.Status,
                    Digest = response.Digest,
                    Total = response.Total,
                    Completed = response.Completed,
                    Progress = response.Total > 0
                        ? (double)response.Completed / response.Total * 100
                        : 0
                };
            }
        }




        /// <inheritdoc />
        public string GetBaseUrl()
        {
            return _configuration.GetBaseUrl();
        }

        private async Task EnsureClientIsInitialized()
        {
            if (_ollamaApiClient == null)
            {
                var baseUrl = _configuration.GetBaseUrl();
                _logger.LogInformation("Creating OllamaApiClient with URL: {Url}", baseUrl);
                _ollamaApiClient = new OllamaApiClient(baseUrl);
                return;
            }
            
            // Check if the base URL has changed
            var currentUrl = GetBaseUrl();
            var clientUrl = _ollamaApiClient.GetType().GetProperty("BaseUrl")?.GetValue(_ollamaApiClient)?.ToString();
            
            if (clientUrl != null && clientUrl != currentUrl)
            {
                _logger.LogInformation("Updating OllamaApiClient URL from {OldUrl} to {NewUrl}", clientUrl, currentUrl);
                
                lock (_lockObject)
                {
                    _ollamaApiClient = new OllamaApiClient(currentUrl);
                }
            }
        }
    }
} 