namespace ConversationService.Infrastructure.Rag.Embedding
{
    public interface ITextEmbeddingGeneration
    {
        string? ModelId { get; set; }
        string? Endpoint { get; set; }
    }
} 