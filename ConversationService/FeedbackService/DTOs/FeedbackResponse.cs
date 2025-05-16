using System;
using System.ComponentModel.DataAnnotations;

namespace ConversationService.FeedbackService.DTOs
{
    public class FeedbackResponse
    {
        public string FeedbackId { get; set; }
        public string ResponseId { get; set; }
        public bool Rating { get; set; }
      
    }
}