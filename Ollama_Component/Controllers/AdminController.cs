using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using Ollama_DB_layer.Entities;
using Ollama_DB_layer.Repositories.AIModelRepo;
using Ollama_Component.Services.AdminServices.Models;
using OllamaSharp.Models;
using Ollama_Component.Services.AdminServices;
using System.Text;
using System.Text.Json;

namespace Ollama_Component.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        public IAdminService AdminService { get; set; }

        public AdminController(IAdminService adminService)
        {
            AdminService = adminService;
        }

        [HttpPost("AddModel")]
        public async Task<IActionResult> AddModel([FromBody] AddModelRequest model)
        {
            if (model == null)
                return BadRequest("Request body cannot be null.");

            var response = await AdminService.AddModelAsync(model);
            return response != null ? Ok(response) : StatusCode(500, "Failed to process the request.");
        }

        [HttpGet("OllamaModelInfo")]
        public async Task<IActionResult> OllamaModelInfo(string ModelName)
        {
            if (string.IsNullOrWhiteSpace(ModelName))
                return BadRequest("Model name cannot be empty.");

            var response = await AdminService.ModelInfoAsync(ModelName);
            return response != null ? Ok(response) : StatusCode(500, "Failed to process the request.");
        }

        [HttpPost("InstallModel")]
        public async Task<IActionResult> InstallModel([FromBody] InstallModelRequest request)
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

                await AdminService.InstallModelAsync(request.ModelName, progress);
                return new EmptyResult();
            }
            else
            {
                var response = await AdminService.InstallModelAsync(request.ModelName);
                return response != null ? Ok(response) : StatusCode(500, "Failed to install model.");
            }
        }

        [HttpGet("InstalledModels")]
        public async Task<IActionResult> InstalledModels([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            if (pageNumber < 1 || pageSize < 1)
                return BadRequest("Invalid pagination parameters.");

            var response = await AdminService.InstalledModelsAsync(pageNumber, pageSize);
            return response != null ? Ok(response) : StatusCode(500, "Failed to process the request.");
        }

        [HttpDelete("SoftDeleteModel")]
        public async Task<IActionResult> SoftDeleteModel([FromQuery] string modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                return BadRequest("Model name cannot be empty.");

            var response = await AdminService.SoftDeleteAIModelAsync(modelName);
            return response != null ? Ok(response) : StatusCode(500, "Failed to process the request.");
        }

        [HttpDelete("UninstallModel")]
        public async Task<IActionResult> UninstallModel([FromBody] RemoveModelRequest model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.ModelName))
                return BadRequest("Model name cannot be empty.");

            var response = await AdminService.UninstllModelAsync(model);
            return response != null ? Ok(response) : StatusCode(500, "Failed to process the request.");
        }
    }
}
