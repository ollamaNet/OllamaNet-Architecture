using Microsoft.Extensions.Logging;
using Ollama_DB_layer.Entities;
using Ollama_DB_layer.UOW;
using ConversationServices.Services.FeedbackService.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ConversationServices.Services.FeedbackService
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FeedbackService> _logger;

        public FeedbackService(IUnitOfWork unitOfWork, ILogger<FeedbackService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }



        public async Task<Feedback> AddFeedbackAsync(AddFeedbackRequest request)
        {
            try
            {
                // Use repository method to check existing feedback by Response_Id
                var existingFeedback = await _unitOfWork.FeedbackRepo.GetFeedbackByResponseIdAsync(request.ResponseId);

                if (existingFeedback != null && !existingFeedback.IsDeleted)
                {
                    throw new InvalidOperationException("This response already has feedback.");
                }

                var feedback = new Feedback
                {
                    Response_Id = request.ResponseId,
                    Rate = request.Rate,
                    IsDeleted = false
                };

                await _unitOfWork.FeedbackRepo.AddAsync(feedback);
                await _unitOfWork.SaveChangesAsync();

                return feedback;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Attempt to add duplicate feedback for response {ResponseId}", request.ResponseId);
                throw;
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error while adding feedback for response {ResponseId}", request.ResponseId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while adding feedback for response {ResponseId}", request.ResponseId);
                throw;
            }
        }





        public async Task<bool> DeleteFeedbackAsync(string responseId, string feedbackId)
        {
            try
            {
                await _unitOfWork.FeedbackRepo.DeleteAsync(responseId, feedbackId);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting feedback {ResponseId} for response {FeedbackId}", responseId, feedbackId);
                throw;
            }
        }

        public async Task<bool> SoftDeleteFeedbackAsync(string responseId, string feedbackId)
        {
            try
            {

                await _unitOfWork.FeedbackRepo.SoftDeleteAsync(responseId, feedbackId);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft deleting feedback {FeedbackId} for response {ResponseId}", responseId, feedbackId);
                throw;
            }
        }

        public async Task<Feedback> UpdateFeedbackAsync(string responseId, string feedbackId, UpdateFeedbackRequest request)
        {
            try
            {
                var feedback = await _unitOfWork.FeedbackRepo.GetByIdAsync(responseId, feedbackId);
                if (feedback == null) throw new KeyNotFoundException($"feedback with ID {feedbackId} not found");

                if (request.Rate != null) feedback.Rate = request.Rate;
               

                await _unitOfWork.FeedbackRepo.UpdateAsync(feedback);
                await _unitOfWork.SaveChangesAsync();
                return feedback;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating response {ResponseId} for feedback {FeedbackId}", responseId, feedbackId);
                throw;
            }
        }

        public async Task<Feedback> GetFeedbackAsync(string responseId, string feedbackId)
        {
            try
            {
                return await _unitOfWork.FeedbackRepo.GetByIdAsync(responseId, feedbackId);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Received request for FeedbackId: {feedbackId}, ResponsekId: {responseId}", feedbackId, responseId);
                throw;
            }
        }

        public async Task<Feedback> GetFeedbackByResponseIdAsync(string responseId)
        {
            try
            {
                return await _unitOfWork.FeedbackRepo.GetFeedbackByResponseIdAsync(responseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting feedback for response {ResponseId}", responseId);
                throw;
            }
        }

        
    }
}