using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Models;
using Ollama_Component.Connectors;
using OllamaSharp;

namespace ChatService
{
    public class SemanticKernelService : ISemanticKernelService
    {
        private readonly OllamaConnector _connector;
        private readonly ChatHistory _chatHistory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SemanticKernelService> _logger;

        private string cacheKey;
            
        public SemanticKernelService(IOllamaApiClient ollamaApiClient, OllamaConnector connector, IMemoryCache cache, ChatHistory chatHistory, ILogger<SemanticKernelService> logger)
        {
            //_chatHistory = new ChatHistory();
            _connector = connector;
            _cache = cache;
            _chatHistory = chatHistory;
            _logger = logger;
        } 


        public async Task<string> GetModelResponse(PromptRequest request)
        {

            cacheKey = request.ConversationId;

            if (request is null)
            {
                throw new ArgumentException("Message cannot be null or empty.", nameof(request));
            }

            if (_cache.TryGetValue(cacheKey, out ChatHistory history))
            {
                _logger.Log(LogLevel.Information, "History Found!");
                _chatHistory.AddRange(history);

            }
            _chatHistory.AddSystemMessage(request.SystemMessage);
            
            // Add user message to chat history
            _chatHistory.AddUserMessage(request.Content);

            var response = await _connector.GetChatMessageContentsAsync(_chatHistory, request.Model);



            // Add the assistant's response to chat history
            if (response.Count > 0)
            {
                _chatHistory.AddAssistantMessage(response[0].Content ?? string.Empty);

                // Cache Options
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(200))
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                .SetPriority(CacheItemPriority.High);
                // Sets chatHistory to cacheKey in cache memory
                _cache.Set(cacheKey, _chatHistory, cacheEntryOptions);

                return response[0].Content ?? string.Empty;
            }

            return "No response from the assistant.";
        }

        public async Task<IAsyncEnumerable<StreamingChatMessageContent>> GetStreamingModelResponse(PromptRequest request)
        {
            if (request is null)
            {
                throw new ArgumentException("Message cannot be null or empty.", nameof(request));
            }

            _chatHistory.AddSystemMessage(request.SystemMessage);
            // Add user message to chat history
            _chatHistory.AddUserMessage(request.Content);

            var response = _connector.GetStreamingChatMessageContentsAsync(_chatHistory, request.Model);

            //// Add the assistant's response to chat history
            //if (response.Count > 0)
            //{
            //    _chatHistory.AddMessage(response[0].Role, response[0].Content ?? string.Empty);
            //    return response[0].Content ?? string.Empty;
            //}
            return response;

        }


    }
}
