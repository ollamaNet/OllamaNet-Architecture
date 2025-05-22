namespace ConversationService.ChatService.RagService
{
    public class PineconeOptions
    {
        public string ApiKey { get; set; }

        public string Cloud { get; set; }
        public string Region { get; set; }

        public string IndexName { get; set; }
    }
}
