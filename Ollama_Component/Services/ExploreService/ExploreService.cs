using Ollama_Component.Mappers.DbMappers;
using Ollama_Component.Services.ExploreService.Models;
using Ollama_DB_layer.UOW;
using System.Security.Principal;

namespace Ollama_Component.Services.ExploreService
{
    public class ExploreService : IExploreService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExploreService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<GetPagedModelsResponse> AvailableModels(GetPagedModelsRequest request)
        {
            var modelsPagedModels = await _unitOfWork.AIModelRepo.AIModelPagination(request.PageNumber, request.Pagesize)
                         ?? throw new InvalidOperationException("Failed to retrieve installed models.");

            var modelsPagedList = new GetPagedModelsResponse 
            {
                models = modelsPagedModels.Items,
                TotalRecords = modelsPagedModels.TotalRecords,
                PageSize = modelsPagedModels.PageSize,
                CurrentPage = modelsPagedModels.CurrentPage,
                TotalPages = modelsPagedModels.TotalPages
            };

            return modelsPagedList;
        }

        public async Task<ModelInfoResponse> ModelInfo(string modelID)
        {
            var DBmodel = await _unitOfWork.AIModelRepo.GetByIdAsync(modelID);
            if (DBmodel == null)
            {
                throw new InvalidOperationException("Failed to retrieve model info.");
            }
            var modelinfo = AIModelMapper.FromModelInfoResposne(DBmodel);

            return modelinfo;
        }




    }
}
