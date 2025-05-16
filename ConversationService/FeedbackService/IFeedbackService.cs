using Ollama_DB_layer.Entities;
using ConversationService.FeedbackService.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConversationService.NoteService.DTOs;

namespace ConversationService.FeedbackService
{
    public interface IFeedbackService
    {
       
        Task<Feedback> AddFeedbackAsync(AddFeedbackRequest request);
        Task<bool> DeleteFeedbackAsync(string feedbackId, string responseId);
        Task<bool> SoftDeleteFeedbackAsync(string feedbackId, string responseId);
        Task<Feedback> UpdateFeedbackAsync(string feedbackId, string responseId, UpdateFeedbackRequest request);
        Task<Feedback> GetFeedbackAsync(string feedbackId, string responseId);
        Task<Feedback> GetFeedbackByResponseIdAsync(string responseId);
    }
}