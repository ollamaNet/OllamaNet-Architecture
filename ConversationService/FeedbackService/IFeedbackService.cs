using Ollama_DB_layer.Entities;
using ConversationService.FeedbackService.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConversationService.NoteService.DTOs;

namespace ConversationService.FeedbackService
{
    public interface IFeedbackService
    {
        // Add note
        Task<Feedback> AddFeedbackAsync(AddFeedbackRequest request);

        // Delete note (hard delete)
        Task<bool> DeleteFeedbackAsync(string feedbackId, string responseId);

        // Soft delete note
        Task<bool> SoftDeleteFeedbackAsync(string feedbackId, string responseId);

        // Update note
        Task<Feedback> UpdateFeedbackAsync(string feedbackId, string responseId, UpdateFeedbackRequest request);

        // Get note by note id and response id
        Task<Feedback> GetFeedbackAsync(string feedbackId, string responseId);

        // Get all notes by response id
        Task<Feedback> GetFeedbackByResponseIdAsync(string responseId);
    }
}