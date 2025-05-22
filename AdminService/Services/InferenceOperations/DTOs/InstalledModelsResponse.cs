using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AdminService.Services.InferenceOperations.DTOs
{
    public class InstalledModelsResponse
    {
        public List<Model> ModelList { get; set; }
    }

    public class Model
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }


        [JsonPropertyName("modified_at")]
        public string ModifiedAt { get; set; }


        [JsonPropertyName("size")]
        public double Size { get; set; }


        [JsonPropertyName("digest")]
        public string Digest { get; set; }


        [JsonPropertyName("details")]
        public ModelDetails Details { get; set; }
    }
    public class ModelDetails
    {

        [JsonPropertyName("format")]
        public string Format { get; set; }


        [JsonPropertyName("family")]
        public string Family { get; set; }


        [JsonPropertyName("families")]
        public List<string>? Families { get; set; }


        [JsonPropertyName("parameter_size")]
        public string Parameter_size { get; set; }


        [JsonPropertyName("quantization_level")]
        public string QuantizationLevel { get; set; }
    }
}
