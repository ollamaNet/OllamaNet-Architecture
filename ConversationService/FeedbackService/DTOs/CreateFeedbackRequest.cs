using System.ComponentModel.DataAnnotations;

namespace ConversationService.FeedbackService.DTOs
{
    public class CreateFeedbackRequest
    {
        [Required]
        public string ResponseId { get; set; }

        public bool Rating { get; set; } // Changed from bool? to bool
    }
}
