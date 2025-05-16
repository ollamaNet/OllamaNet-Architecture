using ConversationService.FeedbackService.DTOs;
using Ollama_DB_layer.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConversationService.FeedbackService
{
    public interface IFeedbackService
    {
        Task<FeedbackResponse> CreateFeedbackAsync(string userId, CreateFeedbackRequest request);
        Task<Feedback> GetFeedbackByIdAsync(string responseId, string feedbackId);
        Task<IEnumerable<FeedbackResponse>> GetFeedbacksByResponseIdAsync(string responseId);
        Task<bool> UpdateFeedbackAsync(string responseId, string feedbackId, UpdateFeedbackRequest request);
        Task<bool> DeleteFeedbackAsync(string responseId, string feedbackId);
    }
}