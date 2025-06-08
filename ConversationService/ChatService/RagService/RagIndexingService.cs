using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.Extensions.Options;
using System.Text;
using Pinecone;
using UglyToad.PdfPig;
using ConversationServices.Services.ChatService.DTOs;

namespace ConversationService.ChatService.RagService
{
    public class RagIndexingService : IRagIndexingService
    {

        private readonly PineconeClient _pinecone;
        private readonly ITextEmbeddingGeneration _embeddingService;

        private readonly RagOptions _ragOptions;
        private readonly PineconeOptions _pineconeOptions;


        public RagIndexingService(PineconeClient pinecone, ITextEmbeddingGeneration embeddingService, IOptions<RagOptions> ragOptions, IOptions<PineconeOptions> pineconeOptions)
        {
            _pinecone = pinecone;
            _embeddingService = embeddingService;
            _ragOptions = ragOptions.Value;
            _pineconeOptions = pineconeOptions.Value;
        }


        public async Task IndexDocumentAsync(PromptRequest request)
        {
            // 1. Extract chunks (already indexed) from PDF
            var chunks = ExtractTextFromPdf(request.DocumentUrl);

            if (chunks == null || !chunks.Any())
            {
                Console.WriteLine("No content extracted from the document.");
                return;
            }

            // 2. Embed chunks into vectors
            var vectors = await EmbedDocumentAsync(chunks, request.ConversationId);

            // 3. Upsert vectors into Pinecone
            await Upsertdataintoindex(vectors);
        }



        public class Chunk
        {
            public int IndexOnPage { get; set; }
            public string Text { get; set; }


        }


        public List<Chunk> ExtractTextFromPdf(string filePath, int chunkSize = 500, int overlap = 50)
        {
            var chunks = new List<Chunk>();

            filePath = filePath.Trim(); // Remove extra spaces
            filePath = Path.GetFullPath(filePath); // Resolve full path

            Console.WriteLine($"Attempting to read: {filePath}");

            if (!File.Exists(filePath))
            {
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
                chunks.Add(new Chunk
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

                    chunks.Add(new Chunk
                    {
                        IndexOnPage = index++,
                        Text = chunkText
                    });
                }
            }

            return chunks;
        }


        //new method to embedding
        public async Task<List<Vector>> EmbedDocumentAsync(List<Chunk> chunks, string ConversationId)
        {
            List<Vector> vectors = new();

            // Batch embed all texts
            var embedRequest = new EmbedRequest
            {
                Model = _ragOptions.OllamaEmbeddingModelId,
                Inputs = chunks.Select(c => new EmbedRequestInputsItem { Text = c.Text }).ToList(),
                Parameters = new EmbedRequestParameters
                {
                    InputType = "passage",
                    Truncate = "END"
                }
            };

            var embeddings = await _pinecone.Inference.EmbedAsync(embedRequest);

            var embeddingList = embeddings.Data.ToList();

            for (int i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];
                var embedding = embeddingList[i].Values.Select(Convert.ToSingle).ToArray();

                vectors.Add(new Vector
                {
                    Id = Guid.NewGuid().ToString(),
                    Values = new ReadOnlyMemory<float>(embedding),
                    Metadata = new Metadata
                    {
                        ["text"] = chunk.Text,
                        ["ConversationId"] = ConversationId,
                        ["ChunkIndex"] = chunk.IndexOnPage,
                    }
                });
            }

            return vectors;

        }


        //upsert data into index
        public async Task Upsertdataintoindex(List<Vector> vectors)
        {
            //get index
            var pinecone = new PineconeClient(_pineconeOptions.ApiKey);


            var index = _pinecone.Index(_pineconeOptions.IndexName);
            await index.UpsertAsync(new UpsertRequest
            {
                Vectors = vectors,
                Namespace = _ragOptions.PineconeNamespace
            });
            Console.WriteLine("Data upserted");

        }



    }

}
