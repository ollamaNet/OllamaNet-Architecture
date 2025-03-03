using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ollama_Component.Services.ExploreService;
using Ollama_Component.Services.ExploreService.Models;
using OllamaSharp.Models;

namespace Ollama_Component.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModelsController : ControllerBase
    {
        private readonly IExploreService _exploreService;

        public ModelsController(IExploreService exploreService)
        {
            _exploreService = exploreService ?? throw new ArgumentNullException(nameof(exploreService));
        }


        [HttpPost("Models")]
        public async Task<IActionResult> Models(int PageNumber, int PageSize)
        {
            if (PageSize == null || PageNumber == null )
                return BadRequest("Request body cannot be null.");

            if (PageNumber < 1 || PageSize < 1)
                return BadRequest("PageNumber and PageSize must be greater than zero.");

            var response = await _exploreService.AvailableModels(PageNumber, PageSize);
            return response != null ? Ok(response) : StatusCode(500, "Failed to process the request.");
        }


        [HttpGet("ModelInfo/{modelName}")]
        public async Task<IActionResult> ModelInfo([FromQuery] string modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                return BadRequest("Model name cannot be empty.");

            var response = await _exploreService.ModelInfo(modelName);
            return response != null ? Ok(response) : StatusCode(500, "Failed to process the request.");
        }
    }
}
