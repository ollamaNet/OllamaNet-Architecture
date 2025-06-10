namespace ConversationService.Infrastructure.Rag.Options
{
    public class RagOptions
    {
        /// <summary>
        /// The Pinecone namespace to use for vector storage
        /// </summary>
        public string PineconeNamespace { get; set; } = "default";

        /// <summary>
        /// The Ollama model ID to use for generating embeddings
        /// </summary>
        public string OllamaEmbeddingModelId { get; set; } = "llama2";

        /// <summary>
        /// Number of top results to retrieve from vector search
        /// </summary>
        public int RetrievalTopK { get; set; } = 3;

        /// <summary>
        /// Whether to enable RAG for all prompts by default
        /// </summary>
        public bool EnableRagByDefault { get; set; } = true;

        /// <summary>
        /// The template to use when adding RAG context to the system message
        /// {0} is replaced with the retrieved context
        /// </summary>
        public string RagContextTemplate { get; set; } = "Use the following retrieved context from the user's uploaded documents to answer the query. If the context is not relevant to the query, you may ignore it:\n\n{0}";

        /// <summary>
        /// Minimum similarity score (0-1) for retrieved chunks to be included
        /// </summary>
        public float MinimumSimilarityScore { get; set; } = 0.7f;

        /// <summary>
        /// Whether to include source document information in the context
        /// </summary>
        public bool IncludeSourceInfo { get; set; } = true;
    }
} 