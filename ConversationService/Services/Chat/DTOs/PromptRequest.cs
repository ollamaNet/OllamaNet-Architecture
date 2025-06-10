using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ConversationServices.Services.ChatService.DTOs
{
    /// <summary>
    /// Represents a request to generate a response from the chat model
    /// </summary>
    public class PromptRequest
    {
        /// <summary>
        /// The ID of the conversation this prompt belongs to
        /// </summary>
        [JsonPropertyName("ConversationId")]
        public string ConversationId { get; set; }

        /// <summary>
        /// The model to use for generating the response
        /// </summary>
        [NotMapped]
        [JsonPropertyName("model")]
        public string Model { get; set; }

        /// <summary>
        /// System message to provide context or instructions to the model
        /// </summary>
        [JsonPropertyName("SystemMessage")]
        public string SystemMessage { get; set; }

        /// <summary>
        /// The actual prompt content/user query
        /// </summary>
        [JsonPropertyName("Content")]
        public string Content { get; set; }

        /// <summary>
        /// Model-specific options for the request
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("Options")]
        public Options? Options { get; set; }

        /// <summary>
        /// Additional metadata for the request, including RAG-specific settings:
        /// - EnableRag: "true"/"false" - Enable/disable RAG for this request
        /// - DocumentId: Limit search to specific document
        /// - MaxResults: Override default top-k results
        /// - MinScore: Override minimum similarity score
        /// - IncludeSource: "true"/"false" - Include source info in context
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("Metadata")]
        public Dictionary<string, string>? Metadata { get; set; }

        /// <summary>
        /// Timestamp when the request was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Model-specific options for the request
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Controls randomness in the model's output (0.0 to 1.0)
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("temperature")]
        public float? Temperature { get; set; }

        /// <summary>
        /// Number of tokens to keep from the prompt
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("num_keep")]
        public int? NumKeep { get; set; }

        /// <summary>
        /// Random seed for reproducible responses
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("seed")]
        public int? Seed { get; set; }

        /// <summary>
        /// Maximum number of tokens to predict
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("num_predict")]
        public int? NumPredict { get; set; }

        /// <summary>
        /// Number of tokens to look back for repetition
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("repeat_last_n")]
        public int? RepeatLastN { get; set; }

        /// <summary>
        /// Penalty for repeated tokens
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("presence_penalty")]
        public float? PresencePenalty { get; set; }

        /// <summary>
        /// Mirostat sampling algorithm version (0, 1, or 2)
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("mirostat")]
        public int? Mirostat { get; set; }

        /// <summary>
        /// Mirostat target entropy
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("mirostat_tau")]
        public float? MirostatTau { get; set; }

        /// <summary>
        /// Mirostat learning rate
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("mirostat_eta")]
        public float? MirostatEta { get; set; }

        /// <summary>
        /// Sequences where the model should stop generating
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("stop")]
        public string[]? Stop { get; set; }

        /// <summary>
        /// Number of context tokens to consider
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("num_ctx")]
        public int? NumCtx { get; set; }

        /// <summary>
        /// Number of batches to process in parallel
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("num_batch")]
        public int? NumBatch { get; set; }

        /// <summary>
        /// Number of threads to use for processing
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("num_thread")]
        public int? NumThread { get; set; }
    }
}
