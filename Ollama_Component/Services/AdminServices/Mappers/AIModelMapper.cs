using Ollama_Component.Services.AdminServices.DTOs;
using Ollama_DB_layer.Entities;
using OllamaSharp.Models;

namespace Ollama_Component.Services.AdminServices.Mappers
{
    public static class AIModelMapper
    {
        public static AIModel FromOllamaToAIModel(this AddModelRequest addModelRequest, ShowModelResponse OllamaModel, string userId)
        {
            if (addModelRequest == null) throw new ArgumentNullException(nameof(addModelRequest));
            if (OllamaModel == null) throw new ArgumentNullException(nameof(OllamaModel));

            AIModel DBModel = new()
            {
                // Attributes taken from the admin manually
                User_Id = userId,
                Name = addModelRequest.Name,
                Description = addModelRequest.Description,
                Version = addModelRequest.Version,
                Digest = addModelRequest.Digest,
                Size = addModelRequest.Size,
                ReleasedAt = addModelRequest.ReleasedAt,
                ReferenceLink = addModelRequest.ReferenceLink,
                ImageUrl = addModelRequest.ImageUrl,
                SizeLabel = addModelRequest.SizeLabel,
                ModelType = addModelRequest.ModelType,

                // Attributes taken from Ollama/show model
                Format = OllamaModel.Details?.Format,
                ParameterSize = OllamaModel.Details?.ParameterSize,
                QuantizationLevel = OllamaModel.Details?.QuantizationLevel,
                License = OllamaModel.License,
                ModelFile = OllamaModel.Modelfile,
                Template = OllamaModel.Template,
                ParentModel = OllamaModel.Details?.ParentModel,
                Family = OllamaModel.Details?.Family,
                Families = OllamaModel.Details?.Families != null ? new List<string>(OllamaModel.Details.Families) : null,  // Allow null

                Architecture = OllamaModel.Info?.Architecture,

                // Nullable values remain nullable
                FileType = (int)(OllamaModel.Info?.FileType),  // No forced default
                ParameterCount = (long)(OllamaModel.Info?.ParameterCount), // No forced default
                QuantizationVersion = (int)(OllamaModel.Info?.QuantizationVersion), // No forced default


                CreatedAt = DateTime.UtcNow, // Always set this
            };

            return DBModel;
        }


        public static AIModel FromRequestToAIModel(this AddModelRequest addModelRequest, string userId)
        {
            if (addModelRequest == null) throw new ArgumentNullException(nameof(addModelRequest));

            AIModel DBModel = new()
            {
                User_Id = userId,
                Name = addModelRequest.Name,
                Description = addModelRequest.Description,
                Version = addModelRequest.Version,
                Digest = addModelRequest.Digest,
                Size = addModelRequest.Size,
                ReleasedAt = addModelRequest.ReleasedAt,
                ReferenceLink = addModelRequest.ReferenceLink,
                ImageUrl = addModelRequest.ImageUrl,
                Format = addModelRequest.Format,
                ParameterSize = addModelRequest.ParameterSize,
                QuantizationLevel = addModelRequest.QuantizationLevel,
                License = addModelRequest.License,
                ModelFile = addModelRequest.ModelFile,
                Template = addModelRequest.Template,
                ParentModel = addModelRequest.ParentModel,
                Family = addModelRequest.Family,
                Families = addModelRequest.Families != null ? new List<string>(addModelRequest.Families) : new List<string>(),
                Languages = addModelRequest.Languages != null ? new List<string>(addModelRequest.Languages) : new List<string>(),
                Architecture = addModelRequest.Architecture,
                FileType = addModelRequest.FileType,
                ParameterCount = addModelRequest.ParameterCount,
                QuantizationVersion = addModelRequest.QuantizationVersion,
                SizeLabel = addModelRequest.SizeLabel,
                ModelType = addModelRequest.ModelType,
                CreatedAt = DateTime.UtcNow,
            };

            return DBModel;
        }

    }
}
