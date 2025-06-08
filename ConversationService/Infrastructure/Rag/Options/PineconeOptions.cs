namespace ConversationService.Infrastructure.Rag.Options
{
    public class PineconeOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Cloud { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string IndexName { get; set; } = string.Empty;
    }
}