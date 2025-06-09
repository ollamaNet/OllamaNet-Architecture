using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OllamaSharp.Models;
using Model = OllamaSharp.Models.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ConversationServices.Services.ChatService.DTOs;

namespace ConversationService.Infrastructure.Integration
{
    public interface IOllamaConnector
    {
        IReadOnlyDictionary<string, object?> Attributes { get; }
        
        Task<IReadOnlyList<OllamaModelResponse>> GetChatMessageContentsAsync(
            ChatHistory chatHistory, 
            PromptRequest request, 
            PromptExecutionSettings? executionSettings = null, 
            Kernel? kernel = null, 
            CancellationToken cancellationToken = default);
        
        IAsyncEnumerable<OllamaModelResponse> GetStreamedChatMessageContentsAsync(
            ChatHistory chatHistory, 
            PromptRequest request, 
            PromptExecutionSettings? executionSettings = null, 
            Kernel? kernel = null, 
            CancellationToken cancellationToken = default);
        
        Task<IEnumerable<Model>> GetInstalledModels();
        
        Task<IEnumerable<Model>> GetInstalledModelsPaged(int pageNumber, int PageSize);
        
        Task<ShowModelResponse> GetModelInfo(string modelName);
                
    }
}