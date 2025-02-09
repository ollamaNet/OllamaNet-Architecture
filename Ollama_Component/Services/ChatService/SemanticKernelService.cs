using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Models;
using Ollama_Component.Connectors;
using Ollama_DB_layer.Entities;
using Ollama_DB_layer.Repositories;
using Ollama_DB_layer.Repositories.AIResponseRepo;
using Ollama_DB_layer.Repositories.ConversationRepo;
using Ollama_DB_layer.Repositories.ConversationUserPromptRepo;
using Ollama_DB_layer.Repositories.PromptRepo;
using OllamaSharp;

namespace Ollama_Component.Services.ChatService
{
    public class SemanticKernelService : ISemanticKernelService
    {
        private readonly IOllamaConnector _connector;
        private readonly ChatHistory _chatHistory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SemanticKernelService> _logger;
        private readonly IConversationRepository _conversationRepo;
        private readonly IPromptRepository _promptRepo;
        private readonly IAIResponseRepository _responseRepo;
        private readonly IConversationPromptResponseRepository _convPromptResRepo;

        private string cacheKey;

        public SemanticKernelService(OllamaConnector connector, IMemoryCache cache, ChatHistory chatHistory, ILogger<SemanticKernelService> logger, IPromptRepository promptRepo, IAIResponseRepository responseRepo, IConversationPromptResponseRepository convPromptResRepo, IConversationRepository conversationRepo)
        {
            //_chatHistory = new ChatHistory();
            _connector = connector;
            _cache = cache;
            _chatHistory = chatHistory;
            _logger = logger;
            _conversationRepo = conversationRepo;
            _promptRepo = promptRepo;
            _responseRepo = responseRepo;
            _convPromptResRepo = convPromptResRepo;
        }


        public async Task<string> GetModelResponse(PromptRequest request)
        {
            var conv = await _conversationRepo.GetByIdAsync(request.ConversationId);
            if(conv == null) return "No conversation found with the given id";

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
            if(_chatHistory.Count != 0)
            {
                foreach(var message in _chatHistory)
                {
                    var sysMessage = message.Content;
                    if (message.Role == AuthorRole.System && sysMessage != request.SystemMessage){
                        _chatHistory.Remove(message);
                        _chatHistory.AddSystemMessage(request.SystemMessage);
                        break;
                    }
                }
            }
            else
                _chatHistory.AddSystemMessage(request.SystemMessage);


            // Add user message to chat history
            _chatHistory.AddUserMessage(request.Content);

            var response = await _connector.GetChatMessageContentsAsync(_chatHistory, request);

            // Add the assistant's response to chat history
            if (response.Count > 0)
            {
                _chatHistory.AddAssistantMessage(response[0].Content ?? string.Empty);

                // Temporary Repository Calling
                #region Temporary Repository Calling
                Prompt repoPrompt = new Prompt
                {
                    Content = request.Content,
                };
                if (request.Options != null)
                {
                    repoPrompt.Temprature = request.Options.Temperature.ToString();
                }

                await _promptRepo.AddAsync(repoPrompt);


                AIResponse repoResponse = new AIResponse
                {
                    Content = response[0].Content,
                    Role = response[0].Role.ToString(),
                    TotalDuration = response[0].TotalDuration.ToString(),
                    LoadDuration = response[0].LoadDuration.ToString(),
                    PromptEvalCount = response[0].PromptEvalCount.ToString(),
                    PromptEvalDuration = response[0].PromptEvalDuration.ToString(),
                    EvalCount = response[0].EvalCount.ToString(),
                    EvalDuration = response[0].EvalDuration.ToString(),
                    CreatedAt = DateTime.Now
                };
                await _responseRepo.AddAsync(repoResponse);


                await _convPromptResRepo.AddAsync(new ConversationPromptResponse
                {
                    Conversation_Id = request.ConversationId,
                    Prompt_Id = repoPrompt.Id,
                    Response_Id = repoResponse.Id
                });

                await _promptRepo.SaveChangesAsync();
                await _responseRepo.SaveChangesAsync();
                await _convPromptResRepo.SaveChangesAsync();
                #endregion

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

            var response = _connector.GetStreamingChatMessageContentsAsync(_chatHistory, request);

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
