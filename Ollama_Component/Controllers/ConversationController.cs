using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ollama_Component.Services.ChatService;
using Ollama_Component.Services.ChatService.Models;

namespace Ollama_Component.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        public IChatService _kernelService { get; set; }
        public ConversationController(IChatService Kernelinterface)
        {
            _kernelService = Kernelinterface;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] PromptRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body cannot be null.");
            }

            // Use the Ollama HTTP Client to send the request
            var response = await _kernelService.GetModelResponse(request);

            if (response == null)
            {
                return StatusCode(500, "Failed to process the chat request.");
            }
            return Ok(response);
        }

        [HttpPost("streamChat")]
        public async Task<IActionResult> streamChat([FromBody] PromptRequest request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            if (!request.Stream)
            {
                var response = await _kernelService.GetModelResponse(request);
                if (response == null)
                {
                    return StatusCode(500);
                }

                return Ok(response) ;
            }
            else
            {
                var stream = await _kernelService.GetStreamingModelResponse(request);
                return Ok(stream);
            }

        }



        [HttpPost("embeddings")]
        public async Task<IActionResult> embeddings([FromBody] PromptRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body cannot be null.");
            }

            // Use the Ollama HTTP Client to send the request
            //var response = await _ollamaClient.ChatAsync(request);
            var response = "this is the AI model response of embeddings";

            if (response == null)
            {
                return StatusCode(500, "Failed to process the chat request.");
            }

            return Ok(response);
        }


        [HttpPost("embeddings")]
        public async Task<IActionResult> OpenConversation([FromBody] OpenConversationRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body cannot be null.");
            }

            var response = "logic to create a conversation in the db";

            if (response == null)
            {
                return StatusCode(500, "Failed to process the chat request.");
            }

            return Ok(response);
        }
    }
}
