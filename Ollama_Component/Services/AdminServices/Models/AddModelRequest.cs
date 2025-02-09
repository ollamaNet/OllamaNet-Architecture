using System.ComponentModel.DataAnnotations;

namespace Ollama_Component.Services.AdminServices.Models
{

    public class AddModelRequest
    {

        public string UserId { get; set; }
        public bool FromOllama { get; set; }
        public string Name { get; set; }    

        public string Description { get; set; }

        public string Version { get; set; }

        public string Size { get; set; }

        public string Digest { get; set; }

        public string Format { get; set; }

        public string ParameterSize { get; set; }

        public string QuantizationLevel { get; set; }

        public bool IsDeleted { get; set; } = false;

        public bool IsArchived { get; set; } = false;

        public DateTime ReleasedAt { get; set; }

        public DateTime ModifiedAt { get; set; }

        public string License { get; set; }

        public string ModelFile { get; set; }

        public string Template { get; set; }

        public string ParentModel { get; set; }

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

    }
}
