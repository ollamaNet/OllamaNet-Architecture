using Ollama_Component.Services.ExploreService.DTOs;
using Ollama_DB_layer.Entities;

namespace Ollama_Component.Services.ExploreService.Mappers
{
    public static class ModelMapper
    {
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
                ModelType = DBmodel.ModelType
            };
            return model;

        }


    }
}
