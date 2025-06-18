using System;
using System.Threading.Tasks;
using ConversationServices.Infrastructure.Caching;
using ConversationService.Infrastructure.Messaging.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConversationService.Infrastructure.Configuration;

public interface IInferenceEngineConfiguration
{
    string GetBaseUrl();
    Task UpdateBaseUrl(string newUrl);
    event Action<string> BaseUrlChanged;
}

public class InferenceEngineConfiguration : IInferenceEngineConfiguration
{
    private string _currentBaseUrl;
    private readonly ILogger<InferenceEngineConfiguration> _logger;
    private readonly IRedisCacheService _redisCacheService;
    private readonly IUrlValidator _urlValidator;
    private const string CACHE_KEY = "InferenceEngine:BaseUrl";
    public event Action<string> BaseUrlChanged;



    public InferenceEngineConfiguration(
        IConfiguration configuration, 
        ILogger<InferenceEngineConfiguration> logger,
        IRedisCacheService redisCacheService,
        IUrlValidator urlValidator)
    {
        _logger = logger;
        _redisCacheService = redisCacheService;
        _urlValidator = urlValidator;
        
        // Try to get from cache first, fall back to config
        var cachedUrl = _redisCacheService.GetAsync<string>(CACHE_KEY).Result;
        _currentBaseUrl = cachedUrl ?? configuration["InferenceEngine:BaseUrl"];
        
        _logger.LogInformation("InferenceEngine configured with URL: {Url}", _currentBaseUrl);
    }

    public string GetBaseUrl() => _currentBaseUrl;

    public async Task UpdateBaseUrl(string newUrl)
    {
        if (string.IsNullOrEmpty(newUrl) || _currentBaseUrl == newUrl)
            return;
            
        if (!_urlValidator.IsValid(newUrl))
        {
            _logger.LogWarning("Invalid URL format received: {NewUrl}", newUrl);
            return;
        }
            
        _currentBaseUrl = newUrl;
        await _redisCacheService.SetAsync(CACHE_KEY, newUrl, TimeSpan.FromDays(7));
        _logger.LogInformation("InferenceEngine BaseUrl updated to: {NewUrl}", newUrl);
        
        BaseUrlChanged?.Invoke(newUrl);
    }
} 