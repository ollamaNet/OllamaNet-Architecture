using Ollama_Component.Services.ExploreService.DTOs;
using Ollama_DB_layer.Entities;

namespace Ollama_Component.Services.ExploreService.Mappers
{
    public static class ModelMapper
    {
        public static ModelInfoResponse FromModelInfoResposne(this AIModel DBmodel)
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
                License = DBmodel.License,
                ModelFile = DBmodel.ModelFile,
                Template = DBmodel.Template,
                ParentModel = DBmodel.ParentModel,
                Family = DBmodel.Family,
                Families = DBmodel.Families,
                Languages = DBmodel.Languages,
                Architecture = DBmodel.Architecture,
                FileType = DBmodel.FileType ?? 0, // Fix for CS0266 and CS8629
                ParameterCount = DBmodel.ParameterCount ?? 0,
                QuantizationVersion = DBmodel.QuantizationVersion ?? 0,
                SizeLabel = DBmodel.SizeLabel,
                ModelType = DBmodel.ModelType,
                Tags = DBmodel.ModelTags.Select(modelTag => new GetTagsResponse { Id = modelTag.Tag.Id, Name = modelTag.Tag.Name }).ToList()
            };
            return model;
        }
    }
}
