using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using Ollama_Component.Connectors;
using Ollama_Component.Mappers.ChatMappers;
using Ollama_Component.Services.CacheService;
using Ollama_Component.Services.ChatService.DTOs;

namespace Ollama_Component.Services.ChatService
{
    public class ChatService : IChatService
    {
        private readonly IOllamaConnector _connector;
        private readonly ILogger<ChatService> _logger;
        private readonly ChatHistoryManager _chatHistoryManager;
        private readonly CacheManager _cacheManager;
        private string? cacheKey;

        public ChatService(
            IOllamaConnector connector,
            ILogger<ChatService> logger,
            ChatHistoryManager chatHistoryManager,
            CacheManager cacheManager)
        {
            _connector = connector;
            _logger = logger;
            _chatHistoryManager = chatHistoryManager;
            _cacheManager = cacheManager;
        }

        public async IAsyncEnumerable<OllamaModelResponse> GetStreamedModelResponse(PromptRequest request)
        {
            if (request is null)
                throw new ArgumentException("Message cannot be null or empty.", nameof(request));

            cacheKey = request.ConversationId;
            ChatHistory? history;

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

            List<OllamaModelResponse> responses = new List<OllamaModelResponse>();
            await foreach (var response in _connector.GetStreamedChatMessageContentsAsync(history, request))
            {
                history.AddAssistantMessage(response.Content);
                _cacheManager.SetChatHistory(cacheKey, history);
                responses.Add(response);

                yield return response;
            }

            await _chatHistoryManager.SaveStreamedChatInteractionAsync(request, responses);
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
    }
}
