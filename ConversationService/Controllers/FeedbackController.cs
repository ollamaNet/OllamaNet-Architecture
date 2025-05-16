using ConversationService.FeedbackService;
using ConversationService.FeedbackService.DTOs;
using ConversationService.NoteService;
using ConversationService.NoteService.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenAI.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ConversationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;
        private readonly ILogger<FeedbackController> _logger;
        public FeedbackController(
            IFeedbackService feedbackService , ILogger<FeedbackController> logger)
           
        {
            _feedbackService = feedbackService ;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<FeedbackResponse>> AddFeedback([FromBody] AddFeedbackRequest request)
        {
            try
            {
                var feedback = await _feedbackService.AddFeedbackAsync(request);
                return Ok(FeedbackResponse.FromEntity(feedback));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding feedback");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{responseId}/{feedbackId}")]
        public async Task<ActionResult> DeleteFeedback(string responseId, string feedbackId)
        {
            try
            {
                var result = await _feedbackService.DeleteFeedbackAsync(feedbackId, responseId);
                if (!result) return NotFound();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting feedback");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("soft-delete/{responseId}/{feedbackId}")]
        public async Task<ActionResult> SoftDeleteFeedback(string responseId, string feedbackId)
        {
            try
            {
                var result = await _feedbackService.SoftDeleteFeedbackAsync(feedbackId, responseId);
                if (!result) return NotFound();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft deleting feedback");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{responseId}/{feedbackId}")]
        public async Task<ActionResult<FeedbackResponse>> UpdateFeedback(string responseId, string feedbackId, [FromBody] UpdateFeedbackRequest request)
        {
            try
            {
                var feedback = await _feedbackService.UpdateFeedbackAsync(feedbackId, responseId, request);
                return Ok(FeedbackResponse.FromEntity(feedback));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating feedback");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("response/{responseId}")]
        public async Task<ActionResult<FeedbackResponse>> GetFeedbackByResponseId(string responseId)
        {
            try
            {
                var feedback = await _feedbackService.GetFeedbackByResponseIdAsync(responseId);
                return Ok(FeedbackResponse.FromEntity(feedback));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notes by response ID");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{responseId}/{FeedbackId}")]
        public async Task<ActionResult<FeedbackResponse>> GetFeedback(string responseId, string feedbackId)
        {
            try
            {
                var feedback = await _feedbackService.GetFeedbackAsync(feedbackId, responseId);
                if (feedback == null) return NotFound();
                return Ok(FeedbackResponse.FromEntity(feedback));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting note");
                return StatusCode(500, "Internal server error");
            }
        }

    }
}