using ConversationServices.Services.ChatService.DTOs;
using Microsoft.Extensions.Options;
using Pinecone;

namespace ConversationService.ChatService.RagService
{
    public class RagRetrievalService : IRagRetrievalService
    {


        private readonly PineconeClient _pinecone;
        private readonly ITextEmbeddingGeneration _embeddingService;
        private readonly RagOptions _ragOptions;
        private readonly PineconeOptions _pineconeOptions;



        public RagRetrievalService(PineconeClient pinecone, ITextEmbeddingGeneration embeddingService, IOptions<RagOptions> ragOptions, IOptions<PineconeOptions> pineconeOptions)
        {
            _pinecone = pinecone;
            _embeddingService = embeddingService;
            _ragOptions = ragOptions.Value;
            _pineconeOptions = pineconeOptions.Value;
        }

        public async Task<List<string>> GetRelevantContextAsync(PromptRequest request)
        {
            //  Clean the user query using your QueryCleaner class
            //  string cleanedQuery = QueryCleaner.CleanQueryAndExtractKeywords(userQuery);
            //Console.WriteLine("Searching");
            //Console.WriteLine(cleanedQuery);

            // Generate the embedding for the query
            var queryEmbedding = (await _pinecone.Inference.EmbedAsync(new EmbedRequest
            {
                Model = _ragOptions.OllamaEmbeddingModelId,
                Inputs = new List<EmbedRequestInputsItem>
                {          //Text=cleanedQuery 
                new EmbedRequestInputsItem { Text =request.Content}
                },
                Parameters = new EmbedRequestParameters
                {
                    InputType = "query",
                    Truncate = "END"
                }
            })).Data.SelectMany(e => e.Values.Select(v => (float)v)).ToList();

            // Get the Pinecone index
            var index = _pinecone.Index(_pineconeOptions.IndexName);

            // Perform vector search
            var queryResponse = await index.QueryAsync(new QueryRequest
            {
                Vector = new ReadOnlyMemory<float>(queryEmbedding.ToArray()),
                Namespace = _ragOptions.PineconeNamespace,
                TopK = 3,
                IncludeValues = false,
                IncludeMetadata = true,
                Filter = new Metadata
                {
                    ["ConversationId"] = new Metadata
                    {
                        ["$eq"] = request.ConversationId
                    }
                }
            });


            List<string> searchResults = queryResponse.Matches.Select(match =>
            {
                var metadata = match.Metadata;

                var content = metadata.TryGetValue("text", out var text) ? text?.ToString() : "No content";
                var chunkIndex = metadata.TryGetValue("ChunkIndex", out var chunkValue) ? chunkValue?.ToString() : "Unknown";

                return $"\nChunk: {chunkIndex}\nContent:\n{content}\n";
            }).ToList();

            Console.WriteLine(searchResults);
            // Return the response
            return searchResults;



        }
    }
}
