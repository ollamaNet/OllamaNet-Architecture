using ConversationService.Infrastructure.Configuration;

namespace ConversationService.Infrastructure.Rag.Embedding
{
    public class OllamaTextEmbeddingGeneration : ITextEmbeddingGeneration
    {
        public string? ModelId { get; }
        public string? Endpoint { get; private set; }
        private readonly IInferenceEngineConfiguration? _configuration;

        public OllamaTextEmbeddingGeneration(string? modelId, string? endpoint)
        {
            ModelId = modelId;
            Endpoint = endpoint;
            _configuration = null;
        }
        
        public OllamaTextEmbeddingGeneration(string? modelId, IInferenceEngineConfiguration configuration)
        {
            ModelId = modelId;
            _configuration = configuration;
            Endpoint = configuration.GetBaseUrl();
            
            // Subscribe to URL changes
            _configuration.BaseUrlChanged += OnBaseUrlChanged;
        }
        
        private void OnBaseUrlChanged(string newUrl)
        {
            Endpoint = newUrl;
        }
    }
}