using ConversationServices.Services.FeedbackService;
using ConversationServices.Services.FeedbackService.DTOs;
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

namespace ConversationServices.Controllers
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<FeedbackResponse>> AddFeedback([FromBody] AddFeedbackRequest request)
        {
            try
            {
                var feedback = await _feedbackService.AddFeedbackAsync(request);
                return Ok(FeedbackResponse.FromEntity(feedback));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Duplicate feedback attempt for response {ResponseId}", request.ResponseId);
                return Conflict(new { message = ex.Message });  // 409 Conflict with message
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding feedback");
                return StatusCode(500, "Internal server error");
            }
        }




        [HttpDelete("{responseId}/{feedbackId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteFeedback(string responseId, string feedbackId)
        {
            try
            {
                var result = await _feedbackService.DeleteFeedbackAsync(responseId, feedbackId);
                if (!result) return NotFound();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting feedback");
                return StatusCode(500, "Internal server error");
            }
        }





        [HttpDelete("soft-delete/{responseId}/{feedbackId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SoftDeleteFeedback(string responseId, string feedbackId)
        {
            try
            {
                var result = await _feedbackService.SoftDeleteFeedbackAsync(responseId, feedbackId);
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<FeedbackResponse>> UpdateFeedback(string responseId, string feedbackId, [FromBody] UpdateFeedbackRequest request)
        {
            try
            {
                var feedback = await _feedbackService.UpdateFeedbackAsync(responseId, feedbackId, request);
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






        [HttpGet("{responseId}/{feedbackId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<FeedbackResponse>> GetFeedback(string responseId, string feedbackId)
        {
            try
            {
                var feedback = await _feedbackService.GetFeedbackAsync(responseId, feedbackId);
                if (feedback == null) return NotFound();
                return Ok(FeedbackResponse.FromEntity(feedback));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting note");
                return StatusCode(500, "Internal server error");
            }
        }





        [HttpGet("response/{responseId}")]

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

    }
}