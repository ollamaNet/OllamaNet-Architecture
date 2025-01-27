using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Models;
using OllamaSharp;

namespace ChatService
{
    public class SemanticKernelService : ISemanticKernelService
    {
        private readonly OllamaConnector _connector;
        private readonly ChatHistory _chatHistory;

        public SemanticKernelService(IOllamaApiClient ollamaApiClient, OllamaConnector connector )
        {
            _chatHistory = new ChatHistory();
            _connector = connector;
        }


        public async Task<string> GetModelResponse(PromptRequest request)
        {
            if (request is null)
            {
                throw new ArgumentException("Message cannot be null or empty.", nameof(request));
            }

            _chatHistory.AddSystemMessage(request.SystemMessage);
            // Add user message to chat history
            _chatHistory.AddUserMessage(request.Content);

            var response = await _connector.GetChatMessageContentsAsync(_chatHistory, request.Model);

            // Add the assistant's response to chat history
            if (response.Count > 0)
            {
                _chatHistory.AddMessage(response[0].Role, response[0].Content ?? string.Empty);
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
