using ConversationService.ConversationService;
using ConversationService.ConversationService.DTOs;
using ConversationService.Controllers.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ConversationService.Controllers
{
    [Route("api/conversations")]
    [ApiController]
    [Authorize]
    public class ConversationController : ControllerBase
    {
        private readonly IConversationService _conversationService;
        private readonly IValidator<OpenConversationRequest> _conversationValidator;
        private readonly IValidator<UpdateConversationRequest> _updateValidator;
        private readonly IHttpContextAccessor _httpContextAccessor;






        public ConversationController(
            IConversationService conversationService,
            IValidator<OpenConversationRequest> conversationValidator,
            IValidator<UpdateConversationRequest> updateValidator,
            IHttpContextAccessor httpContextAccessor)
        {
            _conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
            _conversationValidator = conversationValidator ?? throw new ArgumentNullException(nameof(conversationValidator));
            _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }










        /// <summary>
        /// Creates a new conversation
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] OpenConversationRequest request)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue("UserId");
            if (userId == null)
                return Unauthorized();

            var validationResult = await _conversationValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(new { error = "Validation failed", details = validationResult.Errors });  

            var response = await _conversationService.CreateConversationAsync(userId, request);
            if (response == null)
                return StatusCode(500, "Failed to process request");
                
            return CreatedAtAction(nameof(GetById), new { conversationId = response.ConversationId }, response);
        }

        /// <summary>
        /// Gets all conversations for the current user
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 15)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue("UserId");
            if (userId == null)
                return Unauthorized();

            if (!Guid.TryParse(userId, out _))
                return BadRequest(new { error = "Invalid UserId", details = "UserId must be a valid GUID" });

            var response = await _conversationService.GetConversationsAsync(userId, page, pageSize);
            return response == null ? StatusCode(500, "Failed to process request") : Ok(response);
        }







        /// <summary>
        /// Searches conversations by term
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] string term, [FromQuery] int page = 1, [FromQuery] int pageSize = 15)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue("UserId");
            if (userId == null)
                return Unauthorized();

            if (!Guid.TryParse(userId, out _))
                return BadRequest(new { error = "Invalid UserId", details = "UserId must be a valid GUID" });

            if (string.IsNullOrEmpty(term))
                return BadRequest(new { error = "Invalid search term", details = "Search term cannot be empty" });

            var response = await _conversationService.SearchConversationsAsync(userId, term, page, pageSize);
            return response == null ? StatusCode(500, "Failed to process request") : Ok(response);
        }








        /// <summary>
        /// Gets information about a specific conversation
        /// </summary>
        [HttpGet("{conversationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(string conversationId)
        {
            if (!Guid.TryParse(conversationId, out _))
                return BadRequest(new { error = "Invalid ConversationId", details = "ConversationId must be a valid GUID" });

            try 
            {
                var response = await _conversationService.GetConversationInfoAsync(conversationId);
                return response == null ? NotFound() : Ok(response);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
            catch (Exception)
            {
                return StatusCode(500, "Failed to process request");
            }
        }






        /// <summary>
        /// Gets all messages for a specific conversation
        /// </summary>
        [HttpGet("{conversationId}/messages")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMessages(string conversationId)
        {
            if (!Guid.TryParse(conversationId, out _)) 
                return BadRequest(new { error = "Invalid ConversationId", details = "ConversationId must be a valid GUID" });

            try
            {
                var response = await _conversationService.GetConversationMessagesAsync(conversationId);
                return response == null ? NotFound() : Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(500, "Failed to process request");
            }
        }




        /// <summary>
        /// Updates a conversation
        /// </summary>
        [HttpPut("{conversationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(string conversationId, [FromBody] UpdateConversationRequest request)
        {
            if (!Guid.TryParse(conversationId, out _))
                return BadRequest(new { error = "Invalid ConversationId", details = "ConversationId must be a valid GUID" });

            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(new { error = "Validation failed", details = validationResult.Errors });

            try
            {
                var result = await _conversationService.UpdateConversationAsync(conversationId, request);
                if (!result)
                    return NotFound(new { error = "Conversation not found", details = $"No conversation found with ID {conversationId}" });

                return Ok(new { message = "Conversation updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to update conversation", details = ex.Message });
            }
        }












        /// <summary>
        /// Deletes a conversation
        /// </summary>
        [HttpDelete("{conversationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(string conversationId)
        {
            if (!Guid.TryParse(conversationId, out _))
                return BadRequest(new { error = "Invalid ConversationId", details = "ConversationId must be a valid GUID" });

            try
            {
                var result = await _conversationService.DeleteConversationAsync(conversationId);
                if (!result)
                    return NotFound(new { error = "Conversation not found", details = $"No conversation found with ID {conversationId}" });

                return Ok(new { message = "Conversation deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to delete conversation", details = ex.Message });
            }
        }
    }
}
