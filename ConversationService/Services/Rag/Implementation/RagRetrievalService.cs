using Microsoft.Extensions.Options;
using ConversationServices.Services.ChatService.DTOs;
using ConversationService.Infrastructure.Rag.Options;
using ConversationService.Infrastructure.Rag.VectorDb;
using ConversationService.Infrastructure.Rag.Embedding;
using ConversationService.Services.Rag.Interfaces;
using ConversationService.Services.Rag.Helpers;

namespace ConversationService.Services.Rag.Implementation
{
    public class RagRetrievalService : IRagRetrievalService
    {
        private readonly IPineconeService _pineconeService;
        private readonly ITextEmbeddingGeneration _embeddingService;
        private readonly RagOptions _ragOptions;

        public RagRetrievalService(
            IPineconeService pineconeService,
            ITextEmbeddingGeneration embeddingService,
            IOptions<RagOptions> ragOptions)
        {
            _pineconeService = pineconeService;
            _embeddingService = embeddingService;
            _ragOptions = ragOptions.Value;
        }

        public async Task<List<string>> GetRelevantContextAsync(PromptRequest request)
        {
            // Clean the user query
            //string cleanedQuery = QueryCleaner.CleanQueryAndExtractKeywords(request.Content);

            // Generate the embedding for the query
            var queryEmbedding = await _pineconeService.GenerateEmbeddingAsync(
                //cleanedQuery, 
                request.Content,
                _ragOptions.OllamaEmbeddingModelId,
                isQuery: true);

            // Perform vector search
            var queryResponse = await _pineconeService.QueryAsync(
                queryEmbedding,
                _ragOptions.PineconeNamespace,
                request.ConversationId,
                _ragOptions.RetrievalTopK);

            // Format results
            List<string> searchResults = queryResponse.Matches.Select(match =>
            {
                var metadata = match.Metadata;
                var content = metadata.TryGetValue("text", out var text) ? text?.ToString() : "No content";
                var chunkIndex = metadata.TryGetValue("ChunkIndex", out var chunkValue) ? chunkValue?.ToString() : "Unknown";

                return $"\nChunk: {chunkIndex}\nContent:\n{content}\n";
            }).ToList();

            return searchResults;
        }
    }
} 
