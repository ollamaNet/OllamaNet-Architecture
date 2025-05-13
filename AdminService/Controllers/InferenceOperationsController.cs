using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using AdminService.DTOs;

namespace AdminService.Controllers
{
    [Route("api/inference")]
    [ApiController]
    public class InferenceOperationsController : ControllerBase
    {
        private readonly IInferenceOperationsService _inferenceService;

        public InferenceOperationsController(IInferenceOperationsService inferenceService)
        {
            _inferenceService = inferenceService;
        }

        [HttpGet("models/info")]
        public async Task<IActionResult> GetModelInfo([FromQuery] string modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                return BadRequest("Model name cannot be empty.");

            var response = await _inferenceService.ModelInfoAsync(modelName);
            return response != null ? Ok(response) : StatusCode(500, "Failed to retrieve model information.");
        }

        [HttpGet("models")]
        public async Task<IActionResult> GetModels([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            if (pageNumber < 1 || pageSize < 1)
                return BadRequest("Invalid pagination parameters.");

            var response = await _inferenceService.InstalledModelsAsync(pageNumber, pageSize);
            return response != null ? Ok(response) : StatusCode(500, "Failed to retrieve installed models.");
        }

        [HttpPost("models/pull")]
        public async Task<IActionResult> PullModel([FromBody] InstallModelRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ModelName))
                return BadRequest("Model name cannot be empty.");

            if (request.Stream)
            {
                Response.ContentType = "text/event-stream";
                Response.Headers.Append("Cache-Control", "no-cache");
                Response.Headers.Append("Connection", "keep-alive");

                var progress = new Progress<InstallProgressInfo>(async progressInfo =>
                {
                    if (!HttpContext.Response.HasStarted)
                    {
                        var json = JsonSerializer.Serialize(progressInfo);
                        var data = $"data: {json}\n\n";
                        var bytes = Encoding.UTF8.GetBytes(data);
                        await Response.BodyWriter.WriteAsync(bytes);
                        await Response.BodyWriter.FlushAsync();
                    }
                });

                await _inferenceService.InstallModelAsync(request.ModelName, progress);
                return new EmptyResult();
            }
            else
            {
                var response = await _inferenceService.InstallModelAsync(request.ModelName);
                return response != null ? Ok(response) : StatusCode(500, "Failed to install model.");
            }
        }

        [HttpDelete("models")]
        public async Task<IActionResult> UninstallModel([FromBody] RemoveModelRequest model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.ModelName))
                return BadRequest("Model name cannot be empty.");

            var response = await _inferenceService.UninstallModelAsync(model);
            return response != null ? Ok(new { Message = response }) : StatusCode(500, "Failed to uninstall model.");
        }
    }
} 