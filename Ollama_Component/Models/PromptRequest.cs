using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ollama_Console_HttpClient.chatDTos
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

    }
}
