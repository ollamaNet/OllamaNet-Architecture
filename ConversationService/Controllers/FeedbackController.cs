using ConversationService.FeedbackService;
using ConversationService.FeedbackService.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IValidator<CreateFeedbackRequest> _createFeedbackValidator;
        private readonly IValidator<UpdateFeedbackRequest> _updateFeedbackValidator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FeedbackController(
            IFeedbackService feedbackService,
            IValidator<CreateFeedbackRequest> createFeedbackValidator,
            IValidator<UpdateFeedbackRequest> updateFeedbackValidator,
            IHttpContextAccessor httpContextAccessor)
        {
            _feedbackService = feedbackService ?? throw new ArgumentNullException(nameof(feedbackService));
            _createFeedbackValidator = createFeedbackValidator ?? throw new ArgumentNullException(nameof(createFeedbackValidator));
            _updateFeedbackValidator = updateFeedbackValidator ?? throw new ArgumentNullException(nameof(updateFeedbackValidator));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <summary>
        /// Creates a new feedback for a response
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(FeedbackResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreateFeedbackRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    error = "Invalid request model",
                    details = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                });
            }

            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue("UserId");
            if (userId == null)
                return Unauthorized(new { error = "User not authenticated" });

            var validationResult = await _createFeedbackValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(new { error = "Validation failed", details = validationResult.Errors });

            try
            {
                var response = await _feedbackService.CreateFeedbackAsync(userId, request);
                return CreatedAtAction(nameof(GetById), new { feedbackId = response.FeedbackId }, response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets a feedback by ID
        /// </summary>
        [HttpGet("{responseId}/{feedbackId}")]
        [ProducesResponseType(typeof(FeedbackResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string responseId, string feedbackId)
        {
            var feedback = await _feedbackService.GetFeedbackByIdAsync(feedbackId, responseId);
            if (feedback == null)
                return NotFound(new { error = "Feedback not found" });

            return Ok(feedback);
        }

        /// <summary>
        /// Gets all feedbacks for a response
        /// </summary>
        [HttpGet("byResponse/{responseId}")]
        [ProducesResponseType(typeof(IEnumerable<FeedbackResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByResponseId(string responseId)
        {
            var feedbacks = await _feedbackService.GetFeedbacksByResponseIdAsync(responseId);
            return Ok(feedbacks);
        }

        /// <summary>
        /// Updates a feedback
        /// </summary>
        [HttpPut("{responseId}/{feedbackId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(string responseId,  string feedbackId, [FromBody] UpdateFeedbackRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    error = "Invalid request model",
                    details = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                });
            }

            var validationResult = await _updateFeedbackValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(new { error = "Validation failed", details = validationResult.Errors });

            try
            {
                var result = await _feedbackService.UpdateFeedbackAsync(feedbackId, responseId, request);
                if (!result)
                    return NotFound(new { error = "Feedback not found" });

                return Ok(new { message = "Feedback updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a feedback
        /// </summary>
        [HttpDelete("{responseId}/{feedbackId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string responseId, string feedbackId)
        {
            var result = await _feedbackService.DeleteFeedbackAsync(feedbackId,responseId);
            if (!result)
                return NotFound(new { error = "Feedback not found" });

            return Ok(new { message = "Feedback deleted successfully" });
        }
    }
}