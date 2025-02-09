using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OllamaSharp.Models;

namespace Ollama_Component.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModelsController : ControllerBase
    {

        [HttpGet("AvailableModels")]
        public async Task<IActionResult> AvailableModels()
        {
            var response = new List<ModelInfo> { };

            if (response == null)
            {
                return StatusCode(500, "Failed to process the request.");
            }

            return Ok(response);
        }

        public async Task<IActionResult> ModelInfo(string modelName)
        {

            var response = new List<ModelInfo> { };

            if (response == null)
            {
                return StatusCode(500, "Failed to process the request.");
            }

            return Ok(response);
        }
    }
}
