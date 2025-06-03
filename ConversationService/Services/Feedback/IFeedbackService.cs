using Ollama_DB_layer.Entities;
using ConversationServices.Services.FeedbackService.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConversationServices.Services.FeedbackService
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