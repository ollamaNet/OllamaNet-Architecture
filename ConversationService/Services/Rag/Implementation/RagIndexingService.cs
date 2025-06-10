using Microsoft.Extensions.Options;
using ConversationServices.Services.ChatService.DTOs;
using ConversationService.Infrastructure.Rag.Options;
using ConversationService.Infrastructure.Rag.VectorDb;
using ConversationService.Infrastructure.Rag.Embedding;
using ConversationService.Services.Rag.Interfaces;
using ConversationService.Services.Rag.DTOs;
using Pinecone;

namespace ConversationService.Services.Rag.Implementation
{
    public class RagIndexingService : IRagIndexingService
    {
        private readonly IPineconeService _pineconeService;
        private readonly ITextEmbeddingGeneration _embeddingService;
        private readonly RagOptions _ragOptions;

        public RagIndexingService(
            IPineconeService pineconeService,
            ITextEmbeddingGeneration embeddingService,
            IOptions<RagOptions> ragOptions)
        {
            _pineconeService = pineconeService;
            _embeddingService = embeddingService;
            _ragOptions = ragOptions.Value;
        }

        public async Task IndexDocumentAsync(PromptRequest request)
        {
            if (string.IsNullOrEmpty(request.Content))
            {
                throw new ArgumentException("Document content cannot be empty", nameof(request));
            }

            if (string.IsNullOrEmpty(request.ConversationId))
            {
                throw new ArgumentException("Conversation ID is required", nameof(request));
            }

            // Generate vectors for the document content
            var vectors = await EmbedDocumentAsync(request.Content, request.ConversationId);

            // Upsert vectors into Pinecone
            await _pineconeService.UpsertVectorsAsync(vectors, _ragOptions.PineconeNamespace);
        }

        private async Task<List<Vector>> EmbedDocumentAsync(string content, string conversationId)
        {
            List<Vector> vectors = new();

            // Generate embedding for the content
            var embedding = await _pineconeService.GenerateEmbeddingAsync(content, _ragOptions.OllamaEmbeddingModelId);

                vectors.Add(new Vector
                {
                    Id = Guid.NewGuid().ToString(),
                    Values = new ReadOnlyMemory<float>(embedding.ToArray()),
                    Metadata = new Metadata
                    {
                    ["text"] = content,
                        ["ConversationId"] = conversationId,
                    }
                });

            return vectors;
        }
    }
}