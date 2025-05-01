using ExploreService.Mappers;
using ExploreService.DTOs;
using ExploreService.Cache;
using ExploreService.Exceptions;
using ExploreService.Cache.Exceptions;
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

        public async Task<PagedResult<ModelCard>> AvailableModels(int pageNumber, int pageSize)
        {
            _logger.LogInformation("Retrieving available models. PageNumber: {PageNumber}, PageSize: {PageSize}", pageNumber, pageSize);
            var cacheKey = string.Format(CacheKeys.ModelList, pageNumber, pageSize);
            var expiration = TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes);

            try
            {
                return await _cacheManager.GetOrSetAsync(
                    cacheKey,
                    async () => {
                        _logger.LogDebug("Fetching models from database. Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
                        var models = await _unitOfWork.AIModelRepo.AIModelPagination(pageNumber, pageSize);
                        
                        if (models == null)
                        {
                            throw new ModelNotFoundException("Failed to retrieve installed models");
                        }
                        
                        return models;
                    },
                    expiration
                );
            }
            catch (CacheException cacheEx)
            {
                throw ExceptionConverter.ConvertCacheException(cacheEx, "models list");
            }
            catch (Exception ex) when (ex is not ModelNotFoundException && ex is not ExploreServiceException)
            {
                _logger.LogError(ex, "Error retrieving available models. PageNumber: {PageNumber}, PageSize: {PageSize}", 
                    pageNumber, pageSize);
                throw new DataRetrievalException("models list", ex);
            }
        }








        public async Task<ModelInfoResponse> ModelInfo(string modelId)
        {
            _logger.LogInformation("Retrieving model info for modelID: {ModelID}", modelId);
            var cacheKey = string.Format(CacheKeys.ModelInfo, modelId);
            var expiration = TimeSpan.FromMinutes(_settings.ModelInfoExpirationMinutes);

            try
            {
                return await _cacheManager.GetOrSetAsync(
                    cacheKey,
                    async () => {
                        _logger.LogDebug("Fetching model info from database. Model ID: {ModelID}", modelId);
                        var model = await _unitOfWork.AIModelRepo.GetByIdAsync(modelId);
                        
                        if (model == null)
                        {
                            throw new ModelNotFoundException(modelId);
                        }
                        
                        return ModelMapper.FromModelInfoResponse(model);
                    },
                    expiration
                );
            }
            catch (CacheException cacheEx)
            {
                throw ExceptionConverter.ConvertCacheException(cacheEx, $"model '{modelId}'");
            }
            catch (Exception ex) when (ex is not ModelNotFoundException && ex is not ExploreServiceException)
            {
                _logger.LogError(ex, "Error retrieving model info for modelID: {ModelID}", modelId);
                throw new DataRetrievalException($"model '{modelId}'", ex);
            }
        }

        public async Task<IEnumerable<ModelCard>> GetTagModels(string tagId)
        {
            _logger.LogInformation("Retrieving models by tag. TagID: {TagID}", tagId);
            var cacheKey = string.Format(CacheKeys.TagModels, tagId);
            var expiration = TimeSpan.FromMinutes(_settings.TagExpirationMinutes);

            try
            {
                return await _cacheManager.GetOrSetAsync(
                    cacheKey,
                    async () => {
                        _logger.LogDebug("Fetching tag models from database. Tag ID: {TagID}", tagId);
                        var models = await _unitOfWork.AIModelRepo.GetModelsByTagIdAsync(tagId);
                        
                        if (models == null)
                        {
                            throw new TagNotFoundException(tagId);
                        }
                        
                        return models;
                    },
                    expiration
                );
            }
            catch (CacheException cacheEx)
            {
                throw ExceptionConverter.ConvertCacheException(cacheEx, $"models for tag '{tagId}'");
            }
            catch (Exception ex) when (ex is not TagNotFoundException && ex is not ExploreServiceException)
            {
                _logger.LogError(ex, "Error retrieving models for tag ID: {TagID}", tagId);
                throw new DataRetrievalException($"models for tag '{tagId}'", ex);
            }
        }

        public async Task<List<GetTagsResponse>> GetTags()
        {
            _logger.LogInformation("Retrieving all tags");
            var cacheKey = CacheKeys.ModelTags;
            var expiration = TimeSpan.FromMinutes(_settings.TagExpirationMinutes);

            try
            {
                return await _cacheManager.GetOrSetAsync(
                    cacheKey,
                    async () => {
                        _logger.LogDebug("Fetching tags from database");
                        var tags = await _unitOfWork.TagRepo.GetAllAsync();
                        
                        if (tags == null)
                        {
                            throw new DataRetrievalException("tags", "No tags found");
                        }
                        
                        return tags.ToGetTagsResponse();
                    },
                    expiration
                );
            }
            catch (CacheException cacheEx)
            {
                throw ExceptionConverter.ConvertCacheException(cacheEx, "tags");
            }
            catch (Exception ex) when (ex is not ExploreServiceException)
            {
                _logger.LogError(ex, "Error retrieving tags");
                throw new DataRetrievalException("tags", ex);
            }
        }
    }
}
