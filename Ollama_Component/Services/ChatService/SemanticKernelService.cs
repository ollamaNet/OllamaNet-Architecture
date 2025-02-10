using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Ollama_Component.Connectors;
using Ollama_Component.Services.ChatService.Models;
using Ollama_DB_layer.Repositories;
using OllamaSharp;

namespace Ollama_Component.Services.ChatService
{
    public class SemanticKernelService : ISemanticKernelService
    {
        private readonly IOllamaConnector _connector;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SemanticKernelService> _logger;
        private readonly ChatHistoryManager _chatHistoryManager;
        private readonly ChatCacheManager _cacheManager;
        private string cacheKey;

        public SemanticKernelService(
            IOllamaConnector connector,
            IMemoryCache cache,
            ILogger<SemanticKernelService> logger,
            ChatHistoryManager chatHistoryManager,
            ChatCacheManager cacheManager)
        {
            _connector = connector;
            _cache = cache;
            _logger = logger;
            _chatHistoryManager = chatHistoryManager;
            _cacheManager = cacheManager;
        }

        public async Task<string> GetModelResponse(PromptRequest request)
        {
            if (request is null)
                throw new ArgumentException("Message cannot be null or empty.", nameof(request));

            cacheKey = request.ConversationId;
            ChatHistory history;

            if (_cacheManager.TryGetChatHistory(cacheKey, out history))
                _logger.LogInformation("History Found in Cache!");

            else
            {
                history = await _chatHistoryManager.GetChatHistoryAsync(request);
                _cacheManager.SetChatHistory(cacheKey, history);
            }

            
            history.AddSystemMessage(request.SystemMessage);
            history.AddUserMessage(request.Content);

            var response = await _connector.GetChatMessageContentsAsync(history, request);

            if (response.Count > 0)
            {
                #region Save Chat Interaction
                await _chatHistoryManager.SaveChatInteractionAsync(request, response);
                return response[0].Content ?? string.Empty;
                #endregion
            }
            return "No response from the assistant.";
        }



        public async Task<IAsyncEnumerable<StreamingChatMessageContent>> GetStreamingModelResponse(PromptRequest request)
        {
            if (request is null)
            {
                throw new ArgumentException("Message cannot be null or empty.", nameof(request));
            }

            var history = await _chatHistoryManager.GetChatHistoryAsync(request);
            return _connector.GetStreamingChatMessageContentsAsync(history, request);
        }
    }
}
