using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Ollama_Component.Connectors
{
    public interface IOllamaConnector
    {
        IReadOnlyDictionary<string, object?> Attributes { get; }
        Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(ChatHistory chatHistory, string model, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default);
        IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(ChatHistory chatHistory, string model, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default);
    }
}