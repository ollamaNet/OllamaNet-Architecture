using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ollama_Component.Services.ChatService;
using Ollama_Component.Services.ChatService.Models;
using Ollama_Component.Services.ConversationService;
using Ollama_Component.Services.ConversationService.Models;

namespace Ollama_Component.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IConversationService _conversationService;
        private readonly IValidator<PromptRequest> _promptValidator;
        private readonly IValidator<OpenConversationRequest> _conversationValidator;

        public ConversationController(
            IChatService chatService,
            IConversationService conversationService,
            IValidator<PromptRequest> promptValidator,
            IValidator<OpenConversationRequest> conversationValidator)
        {
            _chatService = chatService;
            _conversationService = conversationService;
            _promptValidator = promptValidator;
            _conversationValidator = conversationValidator;
        }

        [HttpPost("Chat")]
        public async Task<IActionResult> Chat([FromBody] PromptRequest request)
        {
            var validationResult = await _promptValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(new { error = "Validation failed", details = validationResult.Errors });

            var response = await _chatService.GetModelResponse(request);
            return response == null ? StatusCode(500, "Failed to process request") : Ok(response);
        }

        [HttpPost("StreamChat")]
        public async Task StreamChat([FromBody] PromptRequest request)
        {
            var validationResult = await _promptValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                Response.StatusCode = 400;
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
                Response.StatusCode = 500;
                await Response.WriteAsync(JsonSerializer.Serialize(new { error = "Streaming failed", details = ex.Message }));
            }
        }

        [HttpPost("OpenConversation")]
        public async Task<IActionResult> OpenConversation([FromBody] OpenConversationRequest request)
        {
            var validationResult = await _conversationValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(new { error = "Validation failed", details = validationResult.Errors });

            var response = await _conversationService.CreateConversationAsync(request);
            return response == null ? StatusCode(500, "Failed to process request") : Ok(response);
        }

        [HttpGet("GetConversations/{userId}")]
        public async Task<IActionResult> GetConversations(string userId)
        {
            if (!Guid.TryParse(userId, out _))
                return BadRequest(new { error = "Invalid UserId", details = "UserId must be a valid GUID" });

            var response = await _conversationService.GetConversationsAsync(userId);
            return response == null ? StatusCode(500, "Failed to process request") : Ok(response);
        }

        [HttpGet("ConversationInfo/{conversationId}")]
        public async Task<IActionResult> ConversationInfo(string conversationId)
        {
            if (!Guid.TryParse(conversationId, out _))
                return BadRequest(new { error = "Invalid ConversationId", details = "ConversationId must be a valid GUID" });

            var response = await _conversationService.GetConversationInfoAsync(conversationId);
            return response == null ? StatusCode(500, "Failed to process request") : Ok(response);
        }

        [HttpGet("ConversationMessages/{conversationId}")]
        public async Task<IActionResult> GetConversationMessages(string conversationId)
        {
            if (!Guid.TryParse(conversationId, out _)) return BadRequest(new { error = "Invalid ConversationId", details = "ConversationId must be a valid GUID" });

            var response = await _conversationService.GetConversationMessagesAsync(conversationId);
            return response == null ? StatusCode(500, "Failed to process request") : Ok(response);
        }
    }

    public class PromptRequestValidator : AbstractValidator<PromptRequest>
    {
        public PromptRequestValidator()
        {
            RuleFor(x => x.ConversationId).NotEmpty().Must(BeValidGuid).WithMessage("ConversationId must be a valid GUID");
            RuleFor(x => x.UserId).NotEmpty().Must(BeValidGuid).WithMessage("UserId must be a valid GUID");
            RuleFor(x => x.Model).NotEmpty();
            RuleFor(x => x.Content).NotEmpty();
            RuleFor(x => x.Options.Temperature).NotEmpty();
        }
        private bool BeValidGuid(string guid) => Guid.TryParse(guid, out _);
    }

    public class OpenConversationRequestValidator : AbstractValidator<OpenConversationRequest>
    {
        public OpenConversationRequestValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().Must(BeValidGuid).WithMessage("UserId must be a valid GUID");
            RuleFor(x => x.ModelName).NotEmpty();
        }
        private bool BeValidGuid(string guid) => Guid.TryParse(guid, out _);
    }
}
