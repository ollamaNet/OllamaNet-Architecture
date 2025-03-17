using Microsoft.SemanticKernel.ChatCompletion;
using Ollama_Component.Controllers;
using Ollama_Component.Services.AdminServices.DTOs;
using Ollama_Component.Services.ExploreService.DTOs;
using Ollama_DB_layer.Entities;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;

namespace Ollama_Component.Mappers.DbMappers
{
    public static class AIModelMapper
    {
        public static AIModel FromOllamaToAIModel(this AddModelRequest addModelRequest, ShowModelResponse OllamaModel)
        {
            if (addModelRequest == null) throw new ArgumentNullException(nameof(addModelRequest));
            if (OllamaModel == null) throw new ArgumentNullException(nameof(OllamaModel));

            AIModel DBModel = new()
            {
                // Attributes taken from the admin manually
                User_Id = addModelRequest.UserId,
                Name = addModelRequest.Name,
                Description = addModelRequest.Description,
                Version = addModelRequest.Version,
                Digest = addModelRequest.Digest,
                Size = addModelRequest.Size,
                ReleasedAt = addModelRequest.ReleasedAt,
                ReferenceLink = addModelRequest.ReferenceLink,

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

                SizeLabel = "sizelable" , 
                ModelType = "modeltype ", 

                CreatedAt = DateTime.UtcNow, // Always set this
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
                ReferenceLink = model.ReferenceLink,
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
                FileType = model.FileType,
                ParameterCount = model.ParameterCount,
                QuantizationVersion = model.QuantizationVersion,
                SizeLabel = model.SizeLabel,
                ModelType = model.ModelType,
                CreatedAt = DateTime.UtcNow,
            };

            return DBModel;
        }

        public static ModelInfoResponse FromModelInfoResposne(this AIModel DBmodel)
        {
            if (DBmodel == null) throw new ArgumentNullException(nameof(DBmodel));

            ModelInfoResponse model = new () 
            {
                Name = DBmodel.Name,
                Description = DBmodel.Description,
                Version = DBmodel.Version,
                Size = DBmodel.Size,
                Digest = DBmodel.Digest,
                Format = DBmodel.Format,
                ParameterSize = DBmodel.ParameterSize,
                QuantizationLevel = DBmodel.QuantizationLevel,
                ReleasedAt = DBmodel.ReleasedAt,
                ReferenceLink = DBmodel.ReferenceLink,
                License = DBmodel.License,
                ModelFile = DBmodel.ModelFile,
                Template = DBmodel.Template,
                ParentModel = DBmodel.ParentModel,
                Family = DBmodel.Family,
                Families = DBmodel.Families,
                Languages = DBmodel.Languages,
                Architecture = DBmodel.Architecture,
                FileType = DBmodel.FileType,
                ParameterCount = DBmodel.ParameterCount,
                QuantizationVersion = DBmodel.QuantizationVersion,
                SizeLabel = DBmodel.SizeLabel,
                ModelType = DBmodel.ModelType
            };
            return model;

        }


    }
}
