using Ollama_DB_layer.Entities;
using System.ComponentModel.DataAnnotations;

namespace Ollama_Component.Services.ExploreService.DTOs
{
    public class ModelInfoResponse
    {
        public string Name { get; set; }    

        public string Description { get; set; }


        [MaxLength(20)]
        public string Version { get; set; }

        [MaxLength(50)]
        public string Size { get; set; }


        [Required]
        [MaxLength(100)]
        public string Digest { get; set; }


        [MaxLength(50)]
        public string Format { get; set; }


        [MaxLength(50)]
        public string ParameterSize { get; set; }


        [MaxLength(50)]
        public string QuantizationLevel { get; set; }

        [Required]
        public DateTime ReleasedAt { get; set; }

        public string ReferenceLink { get; set; }

        [MaxLength(int.MaxValue)]
        public string License { get; set; }


        [MaxLength(int.MaxValue)]
        public string ModelFile { get; set; }


        [MaxLength(int.MaxValue)]
        public string Template { get; set; }


        [MaxLength(255)]
        public string ParentModel { get; set; }


        [MaxLength(100)]
        public string Family { get; set; }

        public List<string>? Families { get; set; } // JSON stored as string (Handled via serialization)

        public List<string>? Languages { get; set; } // JSON stored as string (Handled via serialization)


        [MaxLength(100)]
        public string Architecture { get; set; }

        public int FileType { get; set; }

        public long ParameterCount { get; set; }
        
        public int QuantizationVersion { get; set; }

        [MaxLength(50)]
        public string SizeLabel { get; set; }

        [MaxLength(50)]
        public string ModelType { get; set; }

        public ICollection<GetTagsResponse> Tags { get; set; }

    }
}
