using Microsoft.SemanticKernel.ChatCompletion;
using Models;
using Ollama_Component.Controllers;
using Ollama_DB_layer.Entities;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;

namespace Ollama_Component.Mappers
{
    public static class AIModelMapper
    {
        public static AIModel ToAIModel(this AddModelRequest addModelRequest, ShowModelResponse OllamaModel)
        {
            if (addModelRequest == null) throw new ArgumentNullException(nameof(addModelRequest));

            AIModel DBModel = new()
            {
                User_Id = addModelRequest.UserId,
                Name = addModelRequest.Name,
                Description = addModelRequest.Description,
                Version = addModelRequest.Version,
                Digest = addModelRequest.Digest,
                Size = addModelRequest.Size,
                Format = OllamaModel.Details.Format,
                ParameterSize = OllamaModel.Details.ParameterSize,
                QuantizationLevel = OllamaModel.Details.QuantizationLevel,
                CreatedAt = DateTime.Now,
                ReleasedAt = addModelRequest.ReleasedAt,
            };



            return DBModel;
        }
    }
}
