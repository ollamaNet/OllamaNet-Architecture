using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ollama_DB_layer.Entities;
using Ollama_DB_layer.Repositories.AIModelRepo;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
        public async Task<IActionResult> AddModel(AddModelRequest model)
        {

            var response = await AdminService.AddModelAsync(model);
            if (response == null)
            {
                return StatusCode(500, "Failed to process the chat request.");
            }
            return Ok(response);
        }

        [HttpPost("ModelInfo")]
        public async Task<IActionResult> ModelInfo(string modelName, bool storeModel)
        {
            var response = await AdminService.ModelInfoAsync(modelName);

            if (response == null)
            {
                return StatusCode(500, "Failed to process the chat request.");
            }

            return Ok(response);
        }

        [HttpPost("InstallModel")]
        public async Task<IActionResult> InstallModel(InstallModelRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ModelName))
            {
                return BadRequest("Model name cannot be empty.");
            }

            // Handle streaming responses
            if (request.Stream)
            {
                var responseStream = Response.Body;
                Response.Headers.Append("Content-Type", "text/event-stream");
                Response.Headers.Append("Cache-Control", "no-cache");
                Response.Headers.Append("Connection", "keep-alive");

                var progress = new Progress<InstallProgressInfo>(async progressInfo =>
                {
                    if (!HttpContext.Response.HasStarted)
                    {
                        var json = JsonSerializer.Serialize(progressInfo);
                        var data = $"data: {json}\n\n";
                        var bytes = Encoding.UTF8.GetBytes(data);

                        await responseStream.WriteAsync(bytes, 0, bytes.Length);
                        await responseStream.FlushAsync();
                    }
                });

                await AdminService.InstallModelAsync(request.ModelName, progress);
                return new EmptyResult(); // End the stream when complete
            }
            else
            {
                // Non-streaming response
                var response = await AdminService.InstallModelAsync(request.ModelName);
                return response != null ? Ok(response) : StatusCode(500, "Failed to install model.");
            }
        }




        [HttpGet("InstalledModels")]
        public async Task<IActionResult> InstalledModels()
        {

            var response = await AdminService.InstalledModelsAsync();

            if (response == null)
            {
                return StatusCode(500, "Failed to process the chat request.");
            }
            return Ok(response);
        }


        [HttpDelete("SoftDeleteModel")]
        public async Task<IActionResult> SoftDeleteModel(string modelName)
        {

            var response = await AdminService.SoftDeleteAIModelAsync(modelName);

            if (response == null)
            {
                return StatusCode(500, "Failed to process the chat request.");
            }

            return Ok(response);
        }

        [HttpDelete("UninstallModel")]
        public async Task<IActionResult> UninstallModel(RemoveModelRequest model)
        {
            var response = await AdminService.UninstllModelAsync(model);

            if (response == null)
            {
                return StatusCode(500, "Failed to process the chat request.");
            }

            return Ok(response);
        }

    }
}
