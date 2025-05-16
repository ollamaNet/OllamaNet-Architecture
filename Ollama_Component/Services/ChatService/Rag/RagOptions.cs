namespace Ollama_Component.Services.ChatService.Rag
{
    public class RagOptions
    {

        public string OllamaEmbeddingModelId { get; set; }
        public string OllamaEndpoint { get; set; }
        public string PineconeNamespace { get; set; }
        public int RetrievalTopK { get; set; }


    }
}
