using ExploreService.Mappers;
using ExploreService.DTOs;
using ExploreService.Cache;
using ExploreService.Exceptions;
using Ollama_DB_layer.DataBaseHelpers;
using Ollama_DB_layer.DTOs;
using Ollama_DB_layer.UOW;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace ExploreService
{
    public class ExploreService : IExploreService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheManager _cacheManager;
        private readonly ILogger<ExploreService> _logger;
        private readonly RedisCacheSettings _settings;

        public ExploreService(
            IUnitOfWork unitOfWork,
            ICacheManager cacheManager,
            IOptions<RedisCacheSettings> settings,
            ILogger<ExploreService> logger)
        {
            _unitOfWork = unitOfWork;
            _cacheManager = cacheManager;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<PagedResult<ModelCard>> AvailableModels(int PageNumber, int PageSize)
        {
            _logger.LogInformation("Retrieving available models. PageNumber: {PageNumber}, PageSize: {PageSize}", PageNumber, PageSize);
            var stopwatch = Stopwatch.StartNew();
            
            var cacheKey = string.Format(CacheKeys.ModelList, PageNumber, PageSize);
            _logger.LogDebug("Generated cache key: {CacheKey}", cacheKey);
            
            var expiration = TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes);

            try
            {
                var result = await _cacheManager.GetOrSetAsync(
                    cacheKey,
                    async () =>
                    {
                        _logger.LogDebug("Cache miss for key {CacheKey}", cacheKey);
                        var modelStopwatch = Stopwatch.StartNew();
                        
                        var ModelListPaged = await _unitOfWork.AIModelRepo.AIModelPagination(PageNumber, PageSize);
                        
                        modelStopwatch.Stop();
                        _logger.LogDebug("Database query completed in {ElapsedMilliseconds}ms", modelStopwatch.ElapsedMilliseconds);
                        
                        if (ModelListPaged == null)
                        {
                            _logger.LogWarning("No models returned from database");
                            throw new ModelNotFoundException("Failed to retrieve installed models");
                        }
                        
                        return ModelListPaged;
                    },
                    expiration
                );

                stopwatch.Stop();
                _logger.LogInformation("AvailableModels operation completed in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
                return result;
            }
            catch (Exception ex) when (ex is not ModelNotFoundException)
            {
                _logger.LogError(ex, "Error retrieving available models. PageNumber: {PageNumber}, PageSize: {PageSize}", 
                    PageNumber, PageSize);
                throw new ExploreServiceException("Failed to retrieve available models", ex);
            }
        }







        public async Task<ModelInfoResponse> ModelInfo(string modelID)
        {
            _logger.LogInformation("Retrieving model info for modelID: {ModelID}", modelID);
            var stopwatch = Stopwatch.StartNew();
            
            var cacheKey = string.Format(CacheKeys.ModelInfo, modelID);
            _logger.LogDebug("Generated cache key: {CacheKey}", cacheKey);
            
            var expiration = TimeSpan.FromMinutes(_settings.ModelInfoExpirationMinutes);

            try
            {
                var result = await _cacheManager.GetOrSetAsync(
                    cacheKey,
                    async () =>
                    {
                        _logger.LogDebug("Cache miss for key {CacheKey}", cacheKey);
                        var modelStopwatch = Stopwatch.StartNew();
                        
                        var DBmodel = await _unitOfWork.AIModelRepo.GetByIdAsync(modelID);
                        
                        modelStopwatch.Stop();
                        _logger.LogDebug("Database query completed in {ElapsedMilliseconds}ms", modelStopwatch.ElapsedMilliseconds);
                        
                        if (DBmodel == null)
                        {
                            _logger.LogWarning("Model not found with ID: {ModelID}", modelID);
                            throw new ModelNotFoundException(modelID);
                        }

                        return ModelMapper.FromModelInfoResponse(DBmodel);
                    },
                    expiration
                );

                stopwatch.Stop();
                _logger.LogInformation("ModelInfo operation completed in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
                return result;
            }
            catch (Exception ex) when (ex is not ModelNotFoundException)
            {
                _logger.LogError(ex, "Error retrieving model info for modelID: {ModelID}", modelID);
                throw new ExploreServiceException($"Failed to retrieve model info for '{modelID}'", ex);
            }
        }

        public async Task<IEnumerable<ModelCard>> GetTagModels(string tagId)
        {
            _logger.LogInformation("Retrieving models by tag. TagID: {TagID}", tagId);
            var stopwatch = Stopwatch.StartNew();
            
            var cacheKey = string.Format(CacheKeys.TagModels, tagId);
            _logger.LogDebug("Generated cache key: {CacheKey}", cacheKey);
            
            var expiration = TimeSpan.FromMinutes(_settings.TagExpirationMinutes);

            try
            {
                var result = await _cacheManager.GetOrSetAsync(
                    cacheKey,
                    async () =>
                    {
                        _logger.LogDebug("Cache miss for key {CacheKey}", cacheKey);
                        var modelStopwatch = Stopwatch.StartNew();
                        
                        var modelList = await _unitOfWork.AIModelRepo.GetModelsByTagIdAsync(tagId);
                        
                        modelStopwatch.Stop();
                        _logger.LogDebug("Database query completed in {ElapsedMilliseconds}ms", modelStopwatch.ElapsedMilliseconds);
                        
                        if (modelList == null)
                        {
                            _logger.LogWarning("No models found for tag ID: {TagID}", tagId);
                            throw new TagNotFoundException(tagId);
                        }
                        
                        return modelList;
                    },
                    expiration
                );

                stopwatch.Stop();
                _logger.LogInformation("GetTagModels operation completed in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
                return result;
            }
            catch (Exception ex) when (ex is not TagNotFoundException)
            {
                _logger.LogError(ex, "Error retrieving models for tag ID: {TagID}", tagId);
                throw new ExploreServiceException($"Failed to retrieve models for tag '{tagId}'", ex);
            }
        }

        public async Task<List<GetTagsResponse>> GetTags()
        {
            _logger.LogInformation("Retrieving all tags");
            var stopwatch = Stopwatch.StartNew();
            
            var cacheKey = CacheKeys.ModelTags;
            _logger.LogDebug("Generated cache key: {CacheKey}", cacheKey);
            
            var expiration = TimeSpan.FromMinutes(_settings.TagExpirationMinutes);

            try
            {
                var result = await _cacheManager.GetOrSetAsync(
                    cacheKey,
                    async () =>
                    {
                        _logger.LogDebug("Cache miss for key {CacheKey}", cacheKey);
                        var tagStopwatch = Stopwatch.StartNew();
                        
                        var tags = await _unitOfWork.TagRepo.GetAllAsync();
                        
                        tagStopwatch.Stop();
                        _logger.LogDebug("Database query completed in {ElapsedMilliseconds}ms", tagStopwatch.ElapsedMilliseconds);
                        
                        if (tags == null)
                        {
                            _logger.LogWarning("No tags returned from database");
                            throw new ExploreServiceException("Failed to retrieve tags");
                        }
                        
                        return tags.ToGetTagsResponse();
                    },
                    expiration
                );

                stopwatch.Stop();
                _logger.LogInformation("GetTags operation completed in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tags");
                throw new ExploreServiceException("Failed to retrieve tags", ex);
            }
        }
    }
}
