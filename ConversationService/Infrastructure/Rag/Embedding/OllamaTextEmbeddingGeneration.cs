namespace ConversationService.Infrastructure.Rag.Embedding
{
    public class OllamaTextEmbeddingGeneration : ITextEmbeddingGeneration
    {
        public string? ModelId { get; set; }
        public string? Endpoint { get; set; }

        public OllamaTextEmbeddingGeneration(string? modelId, string? endpoint)
        {
            ModelId = modelId;
            Endpoint = endpoint;
        }
    }
}