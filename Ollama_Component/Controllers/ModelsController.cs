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
        public IExploreService expolerService { get; set; }


        public ModelsController(IExploreService expolre)
        {
            expolerService = expolre;
        }



        [HttpPost("Models")]
        public async Task<IActionResult> Models(GetPagedModelsRequest request)
        {
            var response = await expolerService.AvailableModels(request);

            if (response == null)
            {
                return StatusCode(500, "Failed to process the request.");
            }

            return Ok(response);
        }



        [HttpGet("ModelInfo")]
        public async Task<IActionResult> ModelInfo(string modelName)
        {
            var response = await expolerService.ModelInfo(modelName);

            if (response == null)
            {
                return StatusCode(500, "Failed to process the request.");
            }

            return Ok(response);
        }
    }
}
