using ConversationServices.Services.ChatService;
using ConversationServices.Services.ChatService.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConversationServices.Controllers
{
    [Route("api/chats")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IValidator<PromptRequest> _chatValidator;
        private readonly IHttpContextAccessor _httpContextAccessor;



        public ChatController(
            IChatService chatService,
            IValidator<PromptRequest> chatValidator,
            IHttpContextAccessor httpContextAccessor)
        {
            _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
            _chatValidator = chatValidator ?? throw new ArgumentNullException(nameof(chatValidator));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }







        /// <summary>
        /// Sends a chat message and returns a non-streaming response
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendMessage([FromBody] PromptRequest request)
        {
            var userId = Request.Headers["X-User-Id"].ToString();
            if (userId == null)
                return Unauthorized();


            var validationResult = await _chatValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(new { error = "Validation failed", details = validationResult.Errors });

            var response = await _chatService.GetModelResponse(request);
            return response == null ? StatusCode(500, "Failed to process request") : Ok(response);
        }






        /// <summary>
        /// Sends a chat message and returns a streaming response
        /// </summary>
        [HttpPost("stream")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task StreamMessage([FromBody] PromptRequest request)
        {
            var userId = Request.Headers["X-User-Id"].ToString();
            if (userId == null)
            {
                Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            var validationResult = await _chatValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                await Response.WriteAsync(JsonSerializer.Serialize(new { error = "Validation failed", details = validationResult.Errors }));
                return;
            }

            Response.ContentType = "text/event-stream";

            try
            {
                await foreach (var response in _chatService.GetStreamedModelResponse(request))
                {
                    var json = JsonSerializer.Serialize(response);
                    var bytes = Encoding.UTF8.GetBytes($"data: {json}\n\n");
                    await Response.BodyWriter.WriteAsync(bytes);
                    await Response.BodyWriter.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                await Response.WriteAsync(JsonSerializer.Serialize(new { error = "Streaming failed", details = ex.Message }));
            }
        }
    }
}