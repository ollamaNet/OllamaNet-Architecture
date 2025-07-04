﻿using ExploreService.DTOs;
using Ollama_DB_layer.Entities;

namespace ExploreService.Mappers
{
    public static class ModelMapper
    {
        public static ModelInfoResponse FromModelInfoResponse(this AIModel DBmodel)
        {
            if (DBmodel == null) throw new ArgumentNullException(nameof(DBmodel));

            ModelInfoResponse model = new()
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
                ImageUrl = DBmodel.ImageUrl,
                License = DBmodel.License,
                ModelFile = DBmodel.ModelFile,
                Template = DBmodel.Template,
                ParentModel = DBmodel.ParentModel,
                Family = DBmodel.Family,
                Families = DBmodel.Families,
                Languages = DBmodel.Languages,
                Architecture = DBmodel.Architecture,

                FileType = (int)DBmodel.FileType,
                ParameterCount = (long)DBmodel.ParameterCount,
                QuantizationVersion = (int)DBmodel.QuantizationVersion,
                SizeLabel = DBmodel.SizeLabel,
                ModelType = DBmodel.ModelType,
                Tags = DBmodel.ModelTags.Select(modelTag => new GetTagsResponse { Id = modelTag.Tag.Id, Name = modelTag.Tag.Name }).ToList()
            };
            return model;
        }
    }
}
