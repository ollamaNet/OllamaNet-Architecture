using Ollama_DB_layer.Entities;

namespace Ollama_Component.Services.ExploreService.Models
{
    public class GetPagedModelsResponse
    {
        public List<AIModel> models { get; set; }
        public int TotalRecords { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }

    }
}
