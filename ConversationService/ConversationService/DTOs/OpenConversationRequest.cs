namespace ConversationService.ConversationService.DTOs
{
    public class OpenConversationRequest
    {
        //public string UserId { get; set; }
        public string ModelName { get; set; }

        public string Title { get; set; }
        
        public string SystemMessage { get; set; }
    }
}