namespace ConversationService.Infrastructure.Rag.Embedding
{
    public interface ITextEmbeddingGeneration
    {
        string? ModelId { get; }
        string? Endpoint { get; }
    }
} 