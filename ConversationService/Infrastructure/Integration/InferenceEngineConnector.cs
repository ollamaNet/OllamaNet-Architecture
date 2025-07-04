using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Model = OllamaSharp.Models.Model;
using OpenTelemetry.Trace;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using ConversationServices.Services.ChatService.DTOs;
using ConversationServices.Services.ChatService.Mappers;
using ConversationService.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;

namespace ConversationService.Infrastructure.Integration
{
    public class InferenceEngineConnector : IInferenceEngineConnector
    {
        private readonly IOllamaApiClient _ollamaApiClient;
        private readonly IInferenceEngineConfiguration _configuration;
        private readonly ILogger<InferenceEngineConnector> _logger;

        public string BaseUrl => _configuration.GetBaseUrl();

        public InferenceEngineConnector(
            IOllamaApiClient ollamaApiClient,
            IInferenceEngineConfiguration configuration,
            ILogger<InferenceEngineConnector> logger)
        {
            _ollamaApiClient = ollamaApiClient ?? throw new ArgumentNullException(nameof(ollamaApiClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Subscribe to URL changes to log them
            _configuration.BaseUrlChanged += OnBaseUrlChanged;
        }

        private void OnBaseUrlChanged(string newUrl)
        {
            _logger.LogInformation("InferenceEngine URL updated to: {NewUrl}", newUrl);
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

            await foreach (var response in _ollamaApiClient.ChatAsync(req, cancellationToken))
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
            [EnumeratorCancellation] CancellationToken cancellationToken = default
        )
        {
            var req = CreateStreamedChatRequest(chatHistory, request);

            await foreach (var response in _ollamaApiClient.ChatAsync(req, cancellationToken))
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
            var response = await _ollamaApiClient.ListLocalModelsAsync();
            return response;
        }

        public async Task<IEnumerable<Model>> GetInstalledModelsPaged(int pageNumber, int pageSize)
        {
            var response = await _ollamaApiClient.ListLocalModelsAsync();

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
            var modelInfo = await _ollamaApiClient.ShowModelAsync(modelName);
            return modelInfo;
        }

        public async Task<string> RemoveModel(string modelName)
        {
            await _ollamaApiClient.DeleteModelAsync(modelName);

            var models = await _ollamaApiClient.ListLocalModelsAsync();
            var modelExists = models.Any(m => m.Name == modelName);

            return modelExists ? "Model not removed successfully" : "Model removed successfully";
        }
    }
} 