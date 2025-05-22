namespace ConversationService.ChatService.RagService
{
    public class RagOptions
    {
        public string OllamaEmbeddingModelId { get; set; }
        public string OllamaEndpoint { get; set; }
        public string PineconeNamespace { get; set; }
        public int RetrievalTopK { get; set; }

    }
}
