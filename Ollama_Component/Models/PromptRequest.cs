using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Models
{
    public class PromptRequest
    {
        [JsonPropertyName("ConversationId")] 
        public string ConversationId { get; set; }

        [JsonPropertyName("PromptId")]
        public string PromptId { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("SystemMessage")]
        public string SystemMessage{ get; set; }

        [JsonPropertyName("Content")]
        public string Content { get; set; }

        [JsonPropertyName("stream")]
        public bool Stream { get; set; } 

        [JsonPropertyName("options")]
        public Options Options { get; set; } = new Options();
    }

    public class Options
    {
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; } 

        [JsonPropertyName("num_keep")]
        public int NumKeep { get; set; } 

        [JsonPropertyName("seed")]
        public int Seed { get; set; } 

        [JsonPropertyName("num_predict")]
        public int NumPredict { get; set; } 

        [JsonPropertyName("top_k")]
        public int TopK { get; set; } 

        [JsonPropertyName("top_p")]
        public double TopP { get; set; } 

        [JsonPropertyName("repeat_last_n")]
        public int RepeatLastN { get; set; } 

        [JsonPropertyName("presence_penalty")]
        public double PresencePenalty { get; set; }

        [JsonPropertyName("mirostat")]
        public int Mirostat { get; set; } = 1;

        [JsonPropertyName("mirostat_tau")]
        public double MirostatTau { get; set; } = 0.8;

        [JsonPropertyName("mirostat_eta")]
        public double MirostatEta { get; set; } = 0.6;

        [JsonPropertyName("stop")]
        public List<string> Stop { get; set; } = new List<string> { "\n", "user:" };

        [JsonPropertyName("numa")]
        public bool Numa { get; set; } //Enables NUMA-aware memory allocation Used for multi-CPU systems

        [JsonPropertyName("num_ctx")]
        public int NumCtx { get; set; } // Number of context tokens to consider	Affects response length and quality

        [JsonPropertyName("num_batch")]
        public int NumBatch { get; set; } // Number of parallel batches for processing Higher values increase speed on GPUs

        [JsonPropertyName("num_gpu")]
        public int NumGpu { get; set; } 

        [JsonPropertyName("main_gpu")]
        public int MainGpu { get; set; } = 0;

        [JsonPropertyName("low_vram")]
        public bool LowVram { get; set; } = true;

        [JsonPropertyName("vocab_only")]
        public bool VocabOnly { get; set; } = false; // Loads only the model’s vocabulary	false	Saves memory if generation isn’t needed

        [JsonPropertyName("use_mmap")]
        public bool UseMmap { get; set; } = true;

        [JsonPropertyName("use_mlock")]
        public bool UseMlock { get; set; } = false; // Locks model in RAM to prevent false, Useful for low-latency applications

        [JsonPropertyName("num_thread")]
        public int NumThread { get; set; } = 8;
    }
}
