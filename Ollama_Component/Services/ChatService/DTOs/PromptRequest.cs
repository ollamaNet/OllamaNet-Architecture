using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Ollama_Component.Services.ChatService.DTOs
{
    public class PromptRequest
    {
        [JsonPropertyName("ConversationId")]
        public string ConversationId { get; set; }


        [NotMapped]
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("SystemMessage")]
        public string SystemMessage { get; set; }

        [JsonPropertyName("Content")]
        public string Content { get; set; }

        /*[JsonPropertyName("stream")]
        public bool Stream { get; set; }*/

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("Options")]
        public Options? Options { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class Options
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("temperature")]
        public float? Temperature { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("num_keep")]
        public int? NumKeep { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("seed")]
        public int? Seed { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("num_predict")]
        public int? NumPredict { get; set; }

        /*        [JsonPropertyName("top_k")]
                public int? TopK { get; set; }

                [JsonPropertyName("top_p")]
                public float? TopP { get; set; }*/

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("repeat_last_n")]
        public int? RepeatLastN { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("presence_penalty")]
        public float? PresencePenalty { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("mirostat")]
        public int? Mirostat { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("mirostat_tau")]
        public float? MirostatTau { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("mirostat_eta")]
        public float? MirostatEta { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("stop")]
        public string[]? Stop { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("numa")]
        public bool? Numa { get; set; } //Enables NUMA-aware memory allocation Used for multi-CPU systems

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("num_ctx")]
        public int? NumCtx { get; set; } // Number of context tokens to consider	Affects response length and quality

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("num_batch")]
        public int? NumBatch { get; set; } // Number of parallel batches for processing Higher values increase speed on GPUs

        /*        [JsonPropertyName("num_gpu")]
                public int? NumGpu { get; set; }

                [JsonPropertyName("main_gpu")]
                public int? MainGpu { get; set; } = 0;*/
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("low_vram")]
        public bool? LowVram { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("vocab_only")]
        public bool? VocabOnly { get; set; } // Loads only the model’s vocabulary	false	Saves memory if generation isn’t needed

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("use_mmap")]
        public bool? UseMmap { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("use_mlock")]
        public bool? UseMlock { get; set; } // Locks model in RAM to prevent false, Useful for low-latency applications

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("num_thread")]
        public int? NumThread { get; set; }
    }
}
