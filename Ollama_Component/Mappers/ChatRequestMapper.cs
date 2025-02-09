using Microsoft.SemanticKernel.ChatCompletion;
using Ollama_Component.Services.ChatService.Models;
using OllamaSharp.Models.Chat;

namespace Ollama_Component.Mappers
{
    public static class ChatRequestMapper
    {
        public static ChatRequest ToChatRequest(this PromptRequest request, ChatHistory chatHistory)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var messages = new List<Message>();

            foreach (var message in chatHistory)
            {
                messages.Add(
                    new Message
                    {
                        Role = message.Role == AuthorRole.User ? ChatRole.User : ChatRole.System,
                        Content = message.Content,
                    }
                );
            }

            ChatRequest chatRequest = new()
            {
                Messages = messages,
                Stream = request.Stream,
                Model = request.Model
            };

            if (request.Options != null)
            {
                chatRequest.Options = new OllamaSharp.Models.RequestOptions()
                {
                    Temperature = request.Options.Temperature,
                    NumKeep = request.Options.NumKeep,
                    Seed = request.Options.Seed,
                    NumPredict = request.Options.NumPredict,
                    RepeatLastN = request.Options.RepeatLastN,
                    PresencePenalty = request.Options.PresencePenalty,
                    MiroStat = request.Options.Mirostat,
                    MiroStatEta = request.Options.MirostatEta,
                    MiroStatTau = request.Options.MirostatTau,
                    Stop = request.Options.Stop,
                    Numa = request.Options.Numa,
                    NumCtx = request.Options.NumCtx,
                    NumBatch = request.Options.NumBatch,
                    LowVram = request.Options.LowVram,
                    VocabOnly = request.Options.VocabOnly,
                    UseMmap = request.Options.UseMmap,
                    UseMlock = request.Options.UseMlock,
                    NumThread = request.Options.NumThread
                };
            }

            return chatRequest;
        }

    }
}
