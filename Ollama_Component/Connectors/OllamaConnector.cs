using System.Linq;
using System.Text;
using Azure.Core;
using System.Threading;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Ollama_Component.Services.AdminServices.DTOs;
using Ollama_Component.Services.ChatService.DTOs;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using Model = OllamaSharp.Models.Model;
using OpenTelemetry.Trace;
using Ollama_Component.Services.ChatService.Mappers;

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


        public async Task<IReadOnlyList<OllamaModelResponse>> GetChatMessageContentsAsync(
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
            [ new OllamaModelResponse{
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

        public async IAsyncEnumerable<OllamaModelResponse> GetStreamedChatMessageContentsAsync(
            ChatHistory chatHistory,
            PromptRequest request,
            PromptExecutionSettings? executionSettings = null,
            Kernel? kernel = null,
            CancellationToken cancellationToken = default
        )
        {
            var req = CreateStreamedChatRequest(chatHistory, request);

            await foreach (var response in ollamaApiClient.ChatAsync(req, cancellationToken))
            {
                if (response == null || response.Message == null)
                {
                    continue;
                }

                yield return new OllamaModelResponse
                {
                    Role = GetAuthorRole(response.Message.Role) ?? AuthorRole.Assistant,
                    Content = response.Message.Content ?? string.Empty,
                    ModelId = request.Model
                };
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



        private static ChatRequest CreateStreamedChatRequest(ChatHistory chatHistory, PromptRequest request)
        {
            var chatRequest = request.ToChatRequest(chatHistory, true);

            return chatRequest;
        }
        private static ChatRequest CreateChatRequest(ChatHistory chatHistory, PromptRequest request)
        {
            var chatRequest = request.ToChatRequest(chatHistory, false);

            return chatRequest;
        }



        public async Task<IEnumerable<Model>> GetInstalledModels()
        {
            var response = await ollamaApiClient.ListLocalModelsAsync();

            return response;
        }

        public async Task<IEnumerable<Model>> GetInstalledModelsPaged(int pageNumber, int pageSize)
        {
            var response = await ollamaApiClient.ListLocalModelsAsync();

            // Ensure pageNumber and pageSize are valid
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            // Calculate the items to skip and take for pagination
            var skip = (pageNumber - 1) * pageSize;
            var pagedResponse = response.Skip(skip).Take(pageSize);

            return pagedResponse;
        }

        public async Task<ShowModelResponse> GetModelInfo(string modelName)
        {
            var modelInfo = await ollamaApiClient.ShowModelAsync(modelName);
            return modelInfo;
        }

        public async Task<string> RemoveModel(string modelName)
        {

            await ollamaApiClient.DeleteModelAsync(modelName);

            var models = await ollamaApiClient.ListLocalModelsAsync();
            var modelExists = models.Any(m => m.Name == modelName);

            return modelExists ? "Model not removed successfully" : "Model removed successfully";
        }


        public async IAsyncEnumerable<InstallProgressInfo> PullModelAsync(string modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("Model name cannot be empty.", nameof(modelName));

            await foreach (var response in ollamaApiClient.PullModelAsync(modelName))
            {
                yield return new InstallProgressInfo
                {
                    Status = response.Status,
                    Digest = response.Digest,
                    Total = response.Total,
                    Completed = response.Completed,
                    Progress = response.Total > 0
                        ? (double)response.Completed / response.Total * 100
                        : 0
                };
            }
        }



    }
}