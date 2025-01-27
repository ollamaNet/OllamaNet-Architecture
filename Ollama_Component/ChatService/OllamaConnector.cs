using System.Text;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;

namespace ChatService
{
    public class OllamaConnector 
    {
        private readonly IOllamaApiClient ollamaApiClient;

        public OllamaConnector(IOllamaApiClient ollamaApiClient)
        {
            this.ollamaApiClient = ollamaApiClient;
        }

        public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();


        public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
            ChatHistory chatHistory,
            string model,
            PromptExecutionSettings? executionSettings = null,
            Kernel? kernel = null,
            CancellationToken cancellationToken = default
        )
        {
            var request = CreateChatRequest(chatHistory, model);

            var content = new StringBuilder();
            List<ChatResponseStream> innerContent = [];
            AuthorRole? authorRole = null;

            await foreach (var response in ollamaApiClient.ChatAsync(request, cancellationToken))
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
                    ModelId = model}
            ];
        }

        public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync( ChatHistory chatHistory, string model, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            var request = CreateChatRequest(chatHistory, model);

            await foreach (var response in ollamaApiClient.ChatAsync(request, cancellationToken))
            {
                yield return new StreamingChatMessageContent(
                    role: GetAuthorRole(response.Message.Role) ?? AuthorRole.Assistant,
                    content: response.Message.Content,
                    innerContent: response,
                    modelId: model
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



        private static ChatRequest CreateChatRequest(ChatHistory chatHistory, string model)
        {
            var messages = new List<Message>();

            foreach (var message in chatHistory)
            {
                messages.Add(
                    new Message
                    {
                        Role = message.Role == AuthorRole.User ? ChatRole.User : ChatRole.System,
                        Content = message.Content,
                    }
                );
            }

            return new ChatRequest
            {
                Messages = messages,
                Stream = true,
                Model = model
            };
        }
    }
}
