using Ollama_Component.Services.ExploreService.Mappers;
using Ollama_Component.Services.ExploreService.DTOs;
using Ollama_DB_layer.DataBaseHelpers;
using Ollama_DB_layer.DTOs;
using Ollama_DB_layer.Entities;
using Ollama_DB_layer.UOW;

namespace Ollama_Component.Services.ExploreService
{
    public class ExploreService : IExploreService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExploreService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<ModelCard>> AvailableModels(int PageNumber, int PageSize)
        {
            var ModelListPaged = await _unitOfWork.AIModelRepo.AIModelPagination(PageNumber, PageSize)
                         ?? throw new InvalidOperationException("Failed to retrieve installed models.");


            return ModelListPaged;
        }

        public async Task<ModelInfoResponse> ModelInfo(string modelID)
        {
            var DBmodel = await _unitOfWork.AIModelRepo.GetByIdAsync(modelID);
            if (DBmodel == null)
            {
                throw new InvalidOperationException("Failed to retrieve model info.");
            }

            var modelinfo = ModelMapper.FromModelInfoResposne(DBmodel);

            return modelinfo;
        }

        public async Task<IEnumerable<ModelCard>> GetTagModels(string tagId)
        {
            var modeList = await _unitOfWork.AIModelRepo.GetModelsByTagIdAsync(tagId);
            if (modeList == null)
            {
                throw new InvalidOperationException("Failed to retrieve model info.");
            }

        //    return modeList;
        //}

        public async Task<List<GetTagsResponse>> GetTags()
        {
            var tags = await _unitOfWork.TagRepo.GetAllAsync();
            if (tags == null)
            {
                throw new InvalidOperationException("Failed to retrieve tags.");
            }
            var mappedTags = tags.ToGetTagsResponse();

            return mappedTags;
        }

    }
}
