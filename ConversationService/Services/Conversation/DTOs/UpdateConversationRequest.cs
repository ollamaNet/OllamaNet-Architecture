namespace ConversationServices.Services.ConversationService.DTOs
{
    /// <summary>
    /// Request DTO for updating a conversation
    /// </summary>
    public class UpdateConversationRequest
    {
        /// <summary>
        /// Optional new title for the conversation
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Optional new system message for the conversation
        /// </summary>
        public string SystemMessage { get; set; }
    }
} 