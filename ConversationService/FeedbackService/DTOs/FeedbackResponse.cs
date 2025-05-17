using ConversationService.NoteService.DTOs;
using Ollama_DB_layer.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace ConversationService.FeedbackService.DTOs
{
    public class FeedbackResponse
    {

        public string FeedbackId { get; set; }
        public string ResponseId { get; set; }
        public bool Rate { get; set; }


        public static FeedbackResponse FromEntity(Feedback feedback)
        {
            return new FeedbackResponse
            {
                FeedbackId = feedback.Id,
                ResponseId = feedback.Response_Id,
                Rate = feedback.Rate,
         
            };
        }
    }
}