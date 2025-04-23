using ConversationService.ChatService.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConversationService.ChatService
{
   /// <summary>
   /// Interface for chat service operations
   /// </summary>
   public interface IChatService
   {
       /// <summary>
       /// Gets a model response for a prompt request
       /// </summary>
       Task<ChatResponse> GetModelResponse(PromptRequest request);

       /// <summary>
       /// Gets a streamed model response for a prompt request
       /// </summary>
       IAsyncEnumerable<OllamaModelResponse> GetStreamedModelResponse(PromptRequest request);
   }
}