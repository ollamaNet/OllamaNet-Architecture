using AdminService.Services.AIModelOperations.DTOs;
using Ollama_DB_layer.Entities;
using OllamaSharp.Models;
using AddModelRequest = AdminService.Services.AIModelOperations.DTOs.AddModelRequest;
using CreateModelRequest = AdminService.Services.AIModelOperations.DTOs.CreateModelRequest;

namespace AdminService.Services.AIModelOperations.Mappers
{
    public static class AIModelMapper
    {
        public static AIModel FromOllamaToAIModel(this CreateModelRequest addModelRequest, ShowModelResponse OllamaModel, string userId)
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


        public static AIModel FromRequestToAIModel(this CreateModelRequest Request, string userId)
        {
            if (Request == null) throw new ArgumentNullException(nameof(Request));

            AIModel DBModel = new()
            {
                User_Id = userId,
                Name = Request.Name,
                Description = Request.Description,
                Version = Request.Version,
                Digest = Request.Digest,
                Size = Request.Size,
                ReleasedAt = Request.ReleasedAt,
                ReferenceLink = Request.ReferenceLink,
                ImageUrl = Request.ImageUrl,
                Format = Request.Format,
                ParameterSize = Request.ParameterSize,
                QuantizationLevel = Request.QuantizationLevel,
                License = Request.License,
                ModelFile = Request.ModelFile,
                Template = Request.Template,
                ParentModel = Request.ParentModel,
                Family = Request.Family,
                Families = Request.Families != null ? new List<string>(Request.Families) : new List<string>(),
                Languages = Request.Languages != null ? new List<string>(Request.Languages) : new List<string>(),
                Architecture = Request.Architecture,
                FileType = Request.FileType,
                ParameterCount = Request.ParameterCount,
                QuantizationVersion = Request.QuantizationVersion,
                SizeLabel = Request.SizeLabel,
                ModelType = Request.ModelType,
                CreatedAt = DateTime.UtcNow,
            };

            return DBModel;
        }
        
        public static AIModel UpdateFromRequest(this AIModel existingModel, UpdateModelRequest updateRequest)
        {
            if (existingModel == null) throw new ArgumentNullException(nameof(existingModel));
            if (updateRequest == null) throw new ArgumentNullException(nameof(updateRequest));
            
            // Only update properties that are not null in the request
            if (updateRequest.Description != null) existingModel.Description = updateRequest.Description;
            if (updateRequest.Version != null) existingModel.Version = updateRequest.Version;
            if (updateRequest.Size != null) existingModel.Size = updateRequest.Size;
            if (updateRequest.ReleasedAt != null) existingModel.ReleasedAt = (DateTime)updateRequest.ReleasedAt;
            if (updateRequest.License != null) existingModel.License = updateRequest.License;
            if (updateRequest.Template != null) existingModel.Template = updateRequest.Template;
            if (updateRequest.ModelFile != null) existingModel.ModelFile = updateRequest.ModelFile;
            if (updateRequest.ReferenceLink != null) existingModel.ReferenceLink = updateRequest.ReferenceLink;
            if (updateRequest.ImageUrl != null) existingModel.ImageUrl = updateRequest.ImageUrl;
            if (updateRequest.Digest != null) existingModel.Digest = updateRequest.Digest;
            if (updateRequest.Format != null) existingModel.Format = updateRequest.Format;
            if (updateRequest.ParameterSize != null) existingModel.ParameterSize = updateRequest.ParameterSize;
            if (updateRequest.QuantizationLevel != null) existingModel.QuantizationLevel = updateRequest.QuantizationLevel;
            if (updateRequest.ParentModel != null) existingModel.ParentModel = updateRequest.ParentModel;
            if (updateRequest.Family != null) existingModel.Family = updateRequest.Family;
            if (updateRequest.Families != null) existingModel.Families = updateRequest.Families;
            if (updateRequest.Languages != null) existingModel.Languages = updateRequest.Languages;
            if (updateRequest.Architecture != null) existingModel.Architecture = updateRequest.Architecture;
            if (updateRequest.FileType != null) existingModel.FileType = updateRequest.FileType;
            if (updateRequest.ParameterCount != null) existingModel.ParameterCount = updateRequest.ParameterCount;
            if (updateRequest.QuantizationVersion != null) existingModel.QuantizationVersion = updateRequest.QuantizationVersion;
            if (updateRequest.SizeLabel != null) existingModel.SizeLabel = updateRequest.SizeLabel;
            if (updateRequest.ModelType != null) existingModel.ModelType = updateRequest.ModelType;
            
            // Set last updated timestamp
            //existingModel.UpdatedAt = DateTime.UtcNow;
            
            return existingModel;
        }
        
        public static AIModelResponse ToResponseDto(this AIModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            
            return new AIModelResponse
            {
                Name = model.Name,
                Description = model.Description,
                Version = model.Version,
                Size = model.Size,
                IsActive = model.IsActive,
                CreatedAt = model.CreatedAt,
                ReleasedAt = model.ReleasedAt,
                License = model.License,
                Template = model.Template,
                ModelFile = model.ModelFile,
                ReferenceLink = model.ReferenceLink,
                ImageUrl = model.ImageUrl,
                OwnerId = model.User_Id,
                Digest = model.Digest,
                Format = model.Format,
                ParameterSize = model.ParameterSize,
                QuantizationLevel = model.QuantizationLevel,
                ParentModel = model.ParentModel,
                Family = model.Family,
                Families = model.Families,
                Languages = model.Languages,
                Architecture = model.Architecture,
                FileType = model.FileType,
                ParameterCount = model.ParameterCount,
                QuantizationVersion = model.QuantizationVersion,
                SizeLabel = model.SizeLabel,
                ModelType = model.ModelType
                // Tags would need to be mapped separately
            };
        }
    }
}
