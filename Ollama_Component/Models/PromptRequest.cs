using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Models
{
    public class PromptRequest
    {
        [JsonPropertyName("UserId")]
        public  string UserId { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }


        [JsonPropertyName("SystemMessage")]
        public string SystemMessage { get; set; }

        
        [JsonPropertyName("stream")]
        public bool Stream { get; set; }

        [JsonPropertyName("SessionId")]
        public string SessionId { get; set; }

    }
}
