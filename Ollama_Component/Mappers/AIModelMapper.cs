using Microsoft.SemanticKernel.ChatCompletion;
using Ollama_Component.Controllers;
using Ollama_Component.Services.AdminServices.Models;
using Ollama_DB_layer.Entities;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;

namespace Ollama_Component.Mappers
{
    public static class AIModelMapper
    {
        public static AIModel FromOllamaToAIModel(this AddModelRequest addModelRequest, ShowModelResponse OllamaModel)
        {
            if (addModelRequest == null) throw new ArgumentNullException(nameof(addModelRequest));

            AIModel DBModel = new()
            {
                //attributes taken from the admin manually 
                User_Id = addModelRequest.UserId,
                Name = addModelRequest.Name,
                Description = addModelRequest.Description,
                Version = addModelRequest.Version,
                Digest = addModelRequest.Digest,
                Size = addModelRequest.Size,
                ReleasedAt = addModelRequest.ReleasedAt,

                //attributes taken from ollama/show model
                Format = OllamaModel.Details.Format,
                ParameterSize = OllamaModel.Details.ParameterSize,
                QuantizationLevel = OllamaModel.Details.QuantizationLevel,
                License = OllamaModel.License,
                ModelFile = OllamaModel.Modelfile,
                Template = OllamaModel.Template,
                ParentModel = OllamaModel.Details.ParentModel,
                Family = OllamaModel.Details.Family,
                Families = OllamaModel.Details.Families != null ? new List<string>(OllamaModel.Details.Families) : new List<string>(),
                //Languages = OllamaModel.Details.Languages != null ? new List<string>(OllamaModel.Details.Languages) : new List<string>(),
                Architecture = OllamaModel.Info.Architecture,
                FileType = (int)OllamaModel.Info.FileType,
                ParameterCount = (long)OllamaModel.Info.ParameterCount,
                QuantizationVersion = (int)OllamaModel.Info.QuantizationVersion,
                SizeLabel = " ", //SizeLabel = OllamaModel.Info.SizeLabel,
                ModelType = " ", // ModelType = OllamaModel.Info.ModelType,
                CreatedAt = DateTime.Now,
            };

            return DBModel;
        }

        public static AIModel FromRequestToAIModel(this AddModelRequest model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            AIModel DBModel = new()
            {
                User_Id = model.UserId,
                Name = model.Name,
                Description = model.Description,
                Version = model.Version,
                Digest = model.Digest,
                Size = model.Size,
                ReleasedAt = model.ReleasedAt,

                Format = model.Format,
                ParameterSize = model.ParameterSize,
                QuantizationLevel = model.QuantizationLevel,
                License = model.License,
                ModelFile = model.ModelFile,
                Template = model.Template,
                ParentModel = model.ParentModel,
                Family = model.Family,
                Families = model.Families != null ? new List<string>(model.Families) : new List<string>(),
                Languages = model.Languages != null ? new List<string>(model.Languages) : new List<string>(),
                Architecture = model.Architecture,
                FileType = (int)model.FileType,
                ParameterCount = (long)model.ParameterCount,
                QuantizationVersion = (int)model.QuantizationVersion,
                SizeLabel = model.SizeLabel,
                ModelType = model.ModelType,
                CreatedAt = DateTime.Now,
            };

            return DBModel;
        }
    }
}
