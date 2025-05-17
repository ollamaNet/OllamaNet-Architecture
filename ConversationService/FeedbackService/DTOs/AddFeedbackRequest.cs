using System.ComponentModel.DataAnnotations;

namespace ConversationService.FeedbackService.DTOs
{
    public class AddFeedbackRequest
    {
        [Required]
        public string ResponseId { get; set; }

        public bool Rate { get; set; } 
    }
}
