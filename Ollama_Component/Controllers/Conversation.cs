using ChatService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ollama_Component.Models;
using Models;

namespace Ollama_Component.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Conversation : ControllerBase
    {
        public ISemanticKernelService _kernelService { get; set; }
        public Conversation(ISemanticKernelService Kernelinterface)
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
            var response = await _kernelService.SendMessageAsync(request);

            if (response == null)
            {
                return StatusCode(500, "Failed to process the chat request.");
            }

            return Ok(response);
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
