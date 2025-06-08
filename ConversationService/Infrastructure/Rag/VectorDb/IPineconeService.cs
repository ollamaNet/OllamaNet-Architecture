using Pinecone;

namespace ConversationService.Infrastructure.Rag.VectorDb
{
    public interface IPineconeService
    {
        Task<List<float>> GenerateEmbeddingAsync(string text, string modelId, bool isQuery = false);
        Task UpsertVectorsAsync(List<Vector> vectors, string nameSpace);
        Task<QueryResponse> QueryAsync(List<float> queryEmbedding, string nameSpace, string conversationId, int topK = 3);
    }
} 