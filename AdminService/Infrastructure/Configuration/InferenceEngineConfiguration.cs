using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using AdminService.Infrastructure.Caching.Interfaces;
using AdminService.Infrastructure.Configuration.Options;
using AdminService.Infrastructure.Validation;
using AdminService.Infrastructure.Caching;

namespace AdminService.Infrastructure.Configuration
{
    /// <summary>
    /// Implementation of IInferenceEngineConfiguration for managing inference engine configuration
    /// </summary>
    public class InferenceEngineConfiguration : IInferenceEngineConfiguration
    {
        private string _currentBaseUrl;
        private readonly ILogger<InferenceEngineConfiguration> _logger;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IUrlValidator _urlValidator;
        private readonly InferenceEngineOptions _options;

        /// <inheritdoc />
        public event Action<string> BaseUrlChanged;

        public InferenceEngineConfiguration(
            IConfiguration configuration,
            IRedisCacheService redisCacheService,
            IOptions<InferenceEngineOptions> options,
            IUrlValidator urlValidator,
            ILogger<InferenceEngineConfiguration> logger)
        {
            _logger = logger;
            _redisCacheService = redisCacheService;
            _urlValidator = urlValidator;
            _options = options.Value;

            // Try to get from cache first, fall back to config
            try
            {
                var cachedUrl = _redisCacheService.GetAsync<string>(CacheKeys.InferenceEngineUrl).Result;
                _currentBaseUrl = cachedUrl ?? _options.BaseUrl ?? "http://localhost:11434";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve URL from cache. Using default from configuration.");
                _currentBaseUrl = _options.BaseUrl ?? "http://localhost:11434";
            }

            _logger.LogInformation("InferenceEngine configured with URL: {Url}", _currentBaseUrl);
        }

        /// <inheritdoc />
        public string GetBaseUrl() => _currentBaseUrl;

        /// <inheritdoc />
        public async Task UpdateBaseUrl(string newUrl)
        {
            if (string.IsNullOrEmpty(newUrl) || _currentBaseUrl == newUrl)
                return;

            if (!_urlValidator.IsValidUrl(newUrl))
            {
                _logger.LogWarning("Invalid URL format received: {NewUrl}", newUrl);
                return;
            }

            _currentBaseUrl = newUrl;
            
            try 
            {
                // Store as string in Redis to match the ConversationService implementation
                await _redisCacheService.SetAsync(CacheKeys.InferenceEngineUrl, newUrl, TimeSpan.FromDays(7));
                _logger.LogInformation("InferenceEngine BaseUrl updated to: {NewUrl}", newUrl);
            }
            catch (CacheException ex)
            {
                _logger.LogError(ex, "Failed to cache inference engine URL: {NewUrl}. Operation will continue, but URL won't persist between restarts.", newUrl);
            }

            BaseUrlChanged?.Invoke(newUrl);
        }
    }
} 