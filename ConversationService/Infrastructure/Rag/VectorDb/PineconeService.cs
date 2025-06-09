using Microsoft.Extensions.Options;
using Pinecone;
using ConversationService.Infrastructure.Rag.Options;

namespace ConversationService.Infrastructure.Rag.VectorDb
{
    public class PineconeService : IPineconeService
    {
        private readonly PineconeClient _pinecone;
        private readonly PineconeOptions _options;

        public PineconeService(IOptions<PineconeOptions> options)
        {
            _options = options.Value;
            _pinecone = new PineconeClient(_options.ApiKey);
        }

        public async Task<List<float>> GenerateEmbeddingAsync(string text, string modelId, bool isQuery = false)
        {
            var embedRequest = new EmbedRequest
            {
                Model = modelId,
                Inputs = new List<EmbedRequestInputsItem>
                {
                    new EmbedRequestInputsItem { Text = text }
                },
                Parameters = new EmbedRequestParameters
                {
                    InputType = isQuery ? "query" : "passage",
                    Truncate = "END"
                }
            };

            var embeddings = await _pinecone.Inference.EmbedAsync(embedRequest);
            return embeddings.Data.SelectMany(e => e.Values.Select(v => (float)v)).ToList();
        }

        public async Task UpsertVectorsAsync(List<Vector> vectors, string nameSpace)
        {
            var index = _pinecone.Index(_options.IndexName);
            await index.UpsertAsync(new UpsertRequest
            {
                Vectors = vectors,
                Namespace = nameSpace
            });
        }

        public async Task<QueryResponse> QueryAsync(List<float> queryEmbedding, string nameSpace, string conversationId, int topK = 3)
        {
            var index = _pinecone.Index(_options.IndexName);
            return await index.QueryAsync(new QueryRequest
            {
                Vector = new ReadOnlyMemory<float>(queryEmbedding.ToArray()),
                Namespace = nameSpace,
                TopK = (uint)topK,
                IncludeValues = false,
                IncludeMetadata = true,
                Filter = new Metadata
                {
                    ["ConversationId"] = new Metadata
                    {
                        ["$eq"] = conversationId
                    }
                }
            });
        }
    }
} 