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
        private string cacheKey;

        public SemanticKernelService(
            IOllamaConnector connector,
            IMemoryCache cache,
            ILogger<SemanticKernelService> logger,
            ChatHistoryManager chatHistoryManager)
        {
            _connector = connector;
            _cache = cache;
            _logger = logger;
            _chatHistoryManager = chatHistoryManager;
        }

        public async Task<string> GetModelResponse(PromptRequest request)
        {
            cacheKey = request.ConversationId;

            if (request is null)            
                throw new ArgumentException("Message cannot be null or empty.", nameof(request));

            if (_cache.TryGetValue(cacheKey, out ChatHistory history))
                _logger.Log(LogLevel.Information, "History Found!");

            else
            {
                history = await _chatHistoryManager.GetChatHistoryAsync(request);
                _cache.Set(cacheKey, history, new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(200))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                    .SetPriority(CacheItemPriority.High));
            }

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
