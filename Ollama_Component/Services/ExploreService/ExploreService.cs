using Ollama_Component.Mappers.DbMappers;
using Ollama_Component.Services.ExploreService.Models;
using Ollama_DB_layer.Entities;
using Ollama_DB_layer.Helpers;
using Ollama_DB_layer.Repositories.AIModelRepo;
using System.Security.Principal;

namespace Ollama_Component.Services.ExploreService
{
    public class ExploreService : IExploreService
    {
        public readonly IAIModelRepository _aiModelRepo;

        public ExploreService(IAIModelRepository aIModelRepository)
        {
            _aiModelRepo = aIModelRepository;
        }



        public async Task<GetPagedModelsResponse> AvailableModels(GetPagedModelsRequest request)
        {
            var modelsPagedModels = await _aiModelRepo.AIModelPagination(request.PageNumber, request.Pagesize)
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
            var DBmodel = await _aiModelRepo.GetByIdAsync(modelID);
            if (DBmodel == null)
            {
                throw new InvalidOperationException("Failed to retrieve model info.");
            }
            var modelinfo = AIModelMapper.FromModelInfoResposne(DBmodel);

            return modelinfo;
        }




    }
}
