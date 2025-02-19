using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ollama_Component.Services.ChatService;
using Ollama_Component.Services.ChatService.Models;
using Ollama_Component.Services.ConversationService;
using Ollama_Component.Services.ConversationService.Models;
using System.Text;
using System.Text.Json;

namespace Ollama_Component.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        public readonly IChatService _chatService;
        public readonly IConversationService _conversationService; 

        public ConversationController(IChatService Chatinterface, IConversationService conversationService)
        {
            _chatService = Chatinterface;
            _conversationService = conversationService;
        }

        [HttpPost("Chat")]
        public async Task<IActionResult> Chat([FromBody] PromptRequest request)
        {
            if (request == null)
            {
                return BadRequest();
            }
                var response = await _chatService.GetModelResponse(request);
                if (response == null)
                {
                    return StatusCode(500);
                }

                return Ok(response);
        }



        [HttpPost("StreamChat")]
        public async Task StreamChat([FromBody] PromptRequest request)
        {
            if (request == null)
            {
                Response.StatusCode = 400;
                return;
            }

            Response.ContentType = "text/event-stream";

            await foreach (var response in _chatService.GetStreamedModelResponse(request))
            {
                var json = JsonSerializer.Serialize(response);
                var bytes = Encoding.UTF8.GetBytes($"data: {json}\n\n");
                await Response.BodyWriter.WriteAsync(bytes);
                await Response.BodyWriter.FlushAsync();
            }

        }



        [HttpPost("OpenConversation")]
        public async Task<IActionResult> OpenConversation([FromBody] OpenConversationRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body cannot be null.");
            }

            var response =await  _conversationService.CreateConversationAsync(request);

            if (response == null)
            {
                return StatusCode(500, "Failed to process the chat request.");
            }

            return Ok(response);
        }
    }
}
