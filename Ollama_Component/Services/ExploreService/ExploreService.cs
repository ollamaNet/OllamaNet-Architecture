using Ollama_DB_layer.Repositories.AIModelRepo;

namespace Ollama_Component.Services.ExploreService
{
    public class ExploreService
    {
        public readonly IAIModelRepository _aiModelRepo;

        public ExploreService(IAIModelRepository aIModelRepository)
        {
            _aiModelRepo = aIModelRepository;
        }

        //public async Task<IEnumerable<Model>> GetInstalledModelsAsync(int pageNumber)
        //{
        //    var models = await _aiModelRepo.AIModelPagination(pageNumber)
        //                 ?? throw new InvalidOperationException("Failed to retrieve installed models.");
        //    return models.Any() ? models : throw new InvalidOperationException("No models are installed.");
        //}






    }
}
