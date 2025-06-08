using Microsoft.Extensions.Options;
using System.Text;
using UglyToad.PdfPig;
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
            // 1. Extract chunks from PDF
            var chunks = ExtractTextFromPdf(request.DocumentUrl);

            if (chunks == null || !chunks.Any())
            {
                // TODO: Replace with proper logging
                Console.WriteLine("No content extracted from the document.");
                return;
            }

            // 2. Embed chunks into vectors
            var vectors = await EmbedDocumentAsync(chunks, request.ConversationId);

            // 3. Upsert vectors into Pinecone
            await _pineconeService.UpsertVectorsAsync(vectors, _ragOptions.PineconeNamespace);
        }

        private List<DocumentChunk> ExtractTextFromPdf(string filePath, int chunkSize = 500, int overlap = 50)
        {
            var chunks = new List<DocumentChunk>();

            filePath = filePath.Trim();
            filePath = Path.GetFullPath(filePath);

            if (!File.Exists(filePath))
            {
                // TODO: Replace with proper logging and error handling
                Console.WriteLine("Error: File not found!");
                return chunks;
            }

            StringBuilder text = new StringBuilder();
            using (PdfDocument pdf = PdfDocument.Open(filePath))
            {
                foreach (var page in pdf.GetPages())
                {
                    text.AppendLine(page.Text);
                }
            }

            string fullText = text.ToString();
            int textLength = fullText.Length;
            int index = 0;

            if (textLength <= chunkSize)
            {
                chunks.Add(new DocumentChunk
                {
                    IndexOnPage = index++,
                    Text = fullText
                });
            }
            else
            {
                for (int i = 0; i < textLength; i += chunkSize - overlap)
                {
                    int end = Math.Min(i + chunkSize, textLength);
                    string chunkText = fullText.Substring(i, end - i);

                    chunks.Add(new DocumentChunk
                    {
                        IndexOnPage = index++,
                        Text = chunkText
                    });
                }
            }

            return chunks;
        }

        private async Task<List<Vector>> EmbedDocumentAsync(List<DocumentChunk> chunks, string conversationId)
        {
            List<Vector> vectors = new();

            foreach (var chunk in chunks)
            {
                var embedding = await _pineconeService.GenerateEmbeddingAsync(chunk.Text, _ragOptions.OllamaEmbeddingModelId);

                vectors.Add(new Vector
                {
                    Id = Guid.NewGuid().ToString(),
                    Values = new ReadOnlyMemory<float>(embedding.ToArray()),
                    Metadata = new Metadata
                    {
                        ["text"] = chunk.Text,
                        ["ConversationId"] = conversationId,
                        ["ChunkIndex"] = chunk.IndexOnPage,
                    }
                });
            }

            return vectors;
        }
    }
}