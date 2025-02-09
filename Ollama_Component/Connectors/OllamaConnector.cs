using System.Text;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Models;
using Ollama_Component.Mappers;
using Ollama_Component.Services.ChatService.Models;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;

namespace Ollama_Component.Connectors
{
    public class OllamaConnector : IOllamaConnector
    {
        private readonly IOllamaApiClient ollamaApiClient;

        public OllamaConnector(IOllamaApiClient ollamaApiClient)
        {
            this.ollamaApiClient = ollamaApiClient;
        }

        public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();


        public async Task<IReadOnlyList<ModelResponse>> GetChatMessageContentsAsync(
            ChatHistory chatHistory,
            PromptRequest request,
            PromptExecutionSettings? executionSettings = null,
            Kernel? kernel = null,
            CancellationToken cancellationToken = default
        )
        {
            var req = CreateChatRequest(chatHistory, request);

            var content = new StringBuilder();
            List<ChatResponseStream> innerContent = [];
            AuthorRole? authorRole = null;
            long duration = 0;
            long loadDuration = 0;
            int promptEvalCount = 0;
            long promptEvalDuration = 0;
            int evalCount = 0;
            long evalDuration = 0;

            await foreach (var response in ollamaApiClient.ChatAsync(req, cancellationToken))
            {
                if (response == null || response.Message == null)
                {
                    continue;
                }

                innerContent.Add(response);

                if (response.Message.Content is not null)
                {
                    content.Append(response.Message.Content);
                }

                authorRole = GetAuthorRole(response.Message.Role);

                if (response is ChatDoneResponseStream doneResponse)
                {
                    duration = doneResponse.TotalDuration;
                    loadDuration = doneResponse.LoadDuration;
                    promptEvalCount = doneResponse.PromptEvalCount;
                    promptEvalDuration = doneResponse.PromptEvalDuration;
                    evalCount = doneResponse.EvalCount;
                    evalDuration = doneResponse.EvalDuration;
                }
            }

            return
            [ new ModelResponse{
                    Role = authorRole ?? AuthorRole.Assistant,
                    Content = content.ToString(),
                    InnerContent = innerContent,
                    ModelId = request.Model,
                    TotalDuration = duration,
                    LoadDuration = loadDuration,
                    PromptEvalCount = promptEvalCount,
                    PromptEvalDuration = promptEvalDuration,
                    EvalCount = evalCount,
                    EvalDuration = evalDuration
            }
            ];
        }

        public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync( ChatHistory chatHistory, PromptRequest request, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            var req = CreateChatRequest(chatHistory, request);

            await foreach (var response in ollamaApiClient.ChatAsync(req, cancellationToken))
            {
                yield return new StreamingChatMessageContent(
                    role: GetAuthorRole(response.Message.Role) ?? AuthorRole.Assistant,
                    content: response.Message.Content,
                    innerContent: response,
                    modelId: request.Model
                );
                ;
            }
        }

        private static AuthorRole? GetAuthorRole(ChatRole? role)
        {
            return role?.ToString().ToUpperInvariant() switch
            {
                "USER" => AuthorRole.User,
                "ASSISTANT" => AuthorRole.Assistant,
                "SYSTEM" => AuthorRole.System,
                _ => null
            };
        }



        private static ChatRequest CreateChatRequest(ChatHistory chatHistory, PromptRequest request)
        {
            var chatRequest = request.ToChatRequest(chatHistory);

            return chatRequest;
        }





    }
}
