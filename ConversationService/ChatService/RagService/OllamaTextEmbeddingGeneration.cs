namespace ConversationService.ChatService.RagService
{
    public class OllamaTextEmbeddingGeneration : ITextEmbeddingGeneration
    {
        public string? modelId { get; set; }
        public string? endpoint { get; set; }

        public OllamaTextEmbeddingGeneration(string? modelId, string? endpoint)
        {
            this.modelId = modelId;
            this.endpoint = endpoint;
        }

    }
}
