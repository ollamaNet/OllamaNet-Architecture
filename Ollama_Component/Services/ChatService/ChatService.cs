using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Ollama_Component.Connectors;
using Ollama_Component.Mappers.ChatMappers;
using Ollama_Component.Services.ChatService.Models;
using Ollama_DB_layer.Repositories;
using OllamaSharp;

namespace Ollama_Component.Services.ChatService
{
    public class ChatService : IChatService
    {
        private readonly IOllamaConnector _connector;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ChatService> _logger;
        private readonly ChatHistoryManager _chatHistoryManager;
        private readonly ChatCacheManager _cacheManager;
        private string? cacheKey;

        public ChatService(
            IOllamaConnector connector,
            IMemoryCache cache,
            ILogger<ChatService> logger,
            ChatHistoryManager chatHistoryManager,
            ChatCacheManager cacheManager)
        {
            _connector = connector;
            _cache = cache;
            _logger = logger;
            _chatHistoryManager = chatHistoryManager;
            _cacheManager = cacheManager;
        }

        public async Task<EndpointChatResponse> GetModelResponse(PromptRequest request)
        {
            if (request is null)
                throw new ArgumentException("Message cannot be null or empty.", nameof(request));

            cacheKey = request.ConversationId;
            ChatHistory history;

            //Get Chat History From Cache if Available
            if (_cacheManager.TryGetChatHistory(cacheKey, out history))
                _logger.LogInformation("History Found in Cache!");

            //Retrieves Chat History from Database
            else
            {
                history = await _chatHistoryManager.GetChatHistoryAsync(request);
                _logger.LogInformation("History retrieved From Database");
            }

            //Add Latest User Message and System Message to Chat History
            history.AddSystemMessage(request.SystemMessage);
            history.AddUserMessage(request.Content);


            var ollamaResponse = await _connector.GetChatMessageContentsAsync(history, request);

            var response = ModelResponseMapper.ToModelResponse(ollamaResponse[0], request);

            if (ollamaResponse.Count > 0)
            {
                //Add LLM response to History and Save History to Cache
                history.AddAssistantMessage(ollamaResponse[0].Content ?? string.Empty);
                _cacheManager.SetChatHistory(cacheKey, history);
                await _chatHistoryManager.SaveChatInteractionAsync(request, ollamaResponse);
                return response;
            }

            return response;
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
