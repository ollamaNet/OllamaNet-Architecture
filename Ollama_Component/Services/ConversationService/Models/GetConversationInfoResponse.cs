namespace Ollama_Component.Services.ConversationService.Models
{
    public class GetConversationInfoResponse
    {
        public string ConversationId { get; set; }
        public string ModelName { get; set; }
        public string SystemMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TokenUsage{ get; set; }
    }
}