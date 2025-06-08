namespace ConversationService.Infrastructure.Rag.Options
{
    public class RagOptions
    {
        public string OllamaEmbeddingModelId { get; set; } = string.Empty;
        public string OllamaEndpoint { get; set; } = string.Empty;
        public string PineconeNamespace { get; set; } = string.Empty;
        public int RetrievalTopK { get; set; }
    }
} 