using System.ComponentModel.DataAnnotations;

namespace Ollama_Component.Services.ChatService.DTOs
{
    public class EndpointChatResponse
    {
        public string UserId { get; set; }
        public string ResposneId { get; set; }
        public string ConversationId { get; set; }
        public string ModelName { get; set; }
        public string Content { get; set; }

        public long TotalDuration { get; set; }

        public long LoadDuration { get; set; }

        public int PromptEvalCount { get; set; }

        public long PromptEvalDuration { get; set; }

        public int EvalCount { get; set; }

        [MaxLength(50)]
        public long EvalDuration { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
