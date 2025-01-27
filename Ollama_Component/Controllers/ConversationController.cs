using ChatService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ollama_Component.Models;
using Models;

namespace Ollama_Component.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        public ISemanticKernelService _kernelService { get; set; }
        public ConversationController(ISemanticKernelService Kernelinterface)
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
            var response = await _kernelService.GetStreamingModelResponse(request);

            if (response == null)
            {
                return StatusCode(500, "Failed to process the chat request.");
            }
            return Ok(response);
        }

        [HttpPost("streamChat")]
        public async Task streamChat([FromBody] PromptRequest request)
        {
            if (request == null)
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsync("Request body cannot be null.");
                return;
            }

            if (!request.Stream)
            {
                var response = await _kernelService.GetModelResponse(request);
                if (response == null)
                {
                    HttpContext.Response.StatusCode = 500;
                    await HttpContext.Response.WriteAsync("Failed to process the chat request.");
                    return;
                }

                HttpContext.Response.ContentType = "application/json";
                await HttpContext.Response.WriteAsync(response);
                return;
            }

            // Handle streaming response
            HttpContext.Response.ContentType = "text/event-stream"; // Set for SSE
            var stream = await _kernelService.GetStreamingModelResponse(request);

            await foreach (var message in stream)
            {
                // Write each message as a separate chunk
                await HttpContext.Response.WriteAsync($"data: {message}\n\n");
                await HttpContext.Response.Body.FlushAsync(); // Ensure the client receives it immediately
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

    }
}
