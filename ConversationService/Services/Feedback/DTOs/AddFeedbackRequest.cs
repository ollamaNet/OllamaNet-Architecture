using System.ComponentModel.DataAnnotations;

namespace ConversationServices.Services.FeedbackService.DTOs
{
    public class AddFeedbackRequest
    {
        [Required]
        public string ResponseId { get; set; }

        public bool Rate { get; set; } 
    }
}
