using ExploreService.Mappers;
using ExploreService.DTOs;
using ExploreService.Cache;
using Ollama_DB_layer.DataBaseHelpers;
using Ollama_DB_layer.DTOs;
using Ollama_DB_layer.UOW;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
            var cacheKey = string.Format(CacheKeys.ModelList, PageNumber, PageSize);
            var expiration = TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes);

            return await _cacheManager.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var ModelListPaged = await _unitOfWork.AIModelRepo.AIModelPagination(PageNumber, PageSize)
                        ?? throw new InvalidOperationException("Failed to retrieve installed models.");
                    return ModelListPaged;
                },
                expiration
            );
        }

        public async Task<ModelInfoResponse> ModelInfo(string modelID)
        {
            var cacheKey = string.Format(CacheKeys.ModelInfo, modelID);
            var expiration = TimeSpan.FromMinutes(_settings.ModelInfoExpirationMinutes);

            return await _cacheManager.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var DBmodel = await _unitOfWork.AIModelRepo.GetByIdAsync(modelID);
                    if (DBmodel == null)
                    {
                        throw new InvalidOperationException("Failed to retrieve model info.");
                    }

                    return ModelMapper.FromModelInfoResposne(DBmodel);
                },
                expiration
            );
        }

        public async Task<IEnumerable<ModelCard>> GetTagModels(string tagId)
        {
            var cacheKey = string.Format(CacheKeys.TagModels, tagId);
            var expiration = TimeSpan.FromMinutes(_settings.TagExpirationMinutes);

            return await _cacheManager.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var modeList = await _unitOfWork.AIModelRepo.GetModelsByTagIdAsync(tagId);
                    if (modeList == null)
                    {
                        throw new InvalidOperationException("Failed to retrieve model info.");
                    }
                    return modeList;
                },
                expiration
            );
        }

        public async Task<List<GetTagsResponse>> GetTags()
        {
            var cacheKey = CacheKeys.ModelTags;
            var expiration = TimeSpan.FromMinutes(_settings.TagExpirationMinutes);

            return await _cacheManager.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var tags = await _unitOfWork.TagRepo.GetAllAsync();
                    if (tags == null)
                    {
                        throw new InvalidOperationException("Failed to retrieve tags.");
                    }
                    return tags.ToGetTagsResponse();
                },
                expiration
            );
        }
    }
}
