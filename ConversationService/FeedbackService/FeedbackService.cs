using ConversationService.FeedbackService.DTOs;
using Microsoft.Extensions.Logging;
using Ollama_DB_layer.Entities;
using Ollama_DB_layer.Repositories.AIResponseRepo;
using Ollama_DB_layer.Repositories.FeedbackRepo;
using Ollama_DB_layer.UOW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConversationService.FeedbackService
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
        public async Task<FeedbackResponse> CreateFeedbackAsync(string userId, CreateFeedbackRequest request)
        {
            try
            {
                // Check if the AI response exists 
                var aiResponse = await _unitOfWork.AIResponseRepo.GetByIdAsync(request.ResponseId);
                if (aiResponse == null)
                {
                    _logger.LogWarning("AI Response with ID {ResponseId} not found", request.ResponseId);
                    throw new KeyNotFoundException($"AI Response with ID {request.ResponseId} not found");
                }

                // Create new feedback entity 
                var feedback = new Feedback
                {
                    Id = Guid.NewGuid().ToString(),
                    Response_Id = request.ResponseId,
                    Rate = request.Rating, // Rate is boolean
                    IsDeleted = false
                };

                // Save to database 
                await _unitOfWork.FeedbackRepo.AddAsync(feedback);
                await _unitOfWork.SaveChangesAsync();



                // Return response 
                return MapToFeedbackResponse(feedback);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating feedback for response {ResponseId}", request.ResponseId);
                throw;
            }
        }

        public async Task<bool> DeleteFeedbackAsync(string responseId, string feedbackId)
        {
            try
            {
                var feedback = await _unitOfWork.FeedbackRepo.GetByIdAsync(responseId, feedbackId);
                if (feedback == null)
                {
                    _logger.LogWarning("Feedback with ID {FeedbackId} for response {ResponseId} not found", feedbackId, responseId);
                    return false;
                }

                await _unitOfWork.FeedbackRepo.DeleteAsync(responseId, feedbackId);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting feedback {FeedbackId} for response {ResponseId}", feedbackId, responseId);
                throw;
            }
        }

        public async Task<Feedback> GetFeedbackByIdAsync(string responseId, string feedbackId)
        {

            try
            {
                return await _unitOfWork.FeedbackRepo.GetByIdAsync(responseId, feedbackId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting note {FeedbackId} for response {ResponseId}", responseId, feedbackId);
                throw;
            }
          
        }


        

        public async Task<IEnumerable<FeedbackResponse>> GetFeedbacksByResponseIdAsync(string responseId)
        {
            try
            {
                var feedbacks = await _unitOfWork.FeedbackRepo.GetFeedbacksByResponseIdAsync(responseId);
                return feedbacks.Select(MapToFeedbackResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving feedbacks for response {ResponseId}", responseId);
                throw;
            }
        }

        public async Task<bool> UpdateFeedbackAsync(string feedbackId, string responseId, UpdateFeedbackRequest request)
        {
            try
            {
                var feedback = await _unitOfWork.FeedbackRepo.GetByIdAsync(feedbackId, responseId);
                if (feedback == null)
                {
                    _logger.LogWarning("Feedback with ID {FeedbackId} for response {ResponseId} not found", feedbackId, responseId);
                    return false;
                }

                // Update only provided fields 
                
                    feedback.Rate = request.Rating;
               

                await _unitOfWork.FeedbackRepo.UpdateAsync(feedback);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating feedback {FeedbackId} for response {ResponseId}", feedbackId, responseId);
                throw;
            }
        }

        private FeedbackResponse MapToFeedbackResponse(Feedback feedback)
        {
            return new FeedbackResponse
            {
                FeedbackId = feedback.Id,
                ResponseId = feedback.Response_Id,
                Rating = feedback.Rate
            };
        }
    }
}