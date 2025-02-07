using System.Text;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Models;
using Ollama_Component.Mappers;
using Ollama_Component.Services.AdminServices.Models;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using Model = OllamaSharp.Models.Model;

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


        public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
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
            }

            return
            [ new ChatMessageContent{
                    Role = authorRole ?? AuthorRole.Assistant,
                    Content = content.ToString(),
                    InnerContent = innerContent,
                    ModelId = request.Model}
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



        public async Task<IEnumerable<Model>> GetInstalledModels()
        {
            var response = await ollamaApiClient.ListLocalModelsAsync();

            return response;
        }

        public async Task<ShowModelResponse> GetModelInfo(string modelName)
        {
            var modelInfo = await ollamaApiClient.ShowModelAsync(modelName);
            return modelInfo;
        }
    }
}