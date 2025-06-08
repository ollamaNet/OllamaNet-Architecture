namespace ConversationService.Services.Rag.DTOs
{
    public class DocumentChunk
    {
        public int IndexOnPage { get; set; }
        public string Text { get; set; } = string.Empty;
    }
} 