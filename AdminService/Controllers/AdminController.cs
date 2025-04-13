using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AdminService.DTOs;

namespace AdminService.Controllers
{
    //[Authorize("Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : Controller
    {
        public IAdminService AdminService { get; set; }
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminController(IAdminService adminService, IHttpContextAccessor httpContextAccessor)
        {
            AdminService = adminService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("AddModel")]
        public async Task<IActionResult> AddModel([FromBody] AddModelRequest model)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue("UserId");
            if (userId == null)
                return Unauthorized();

            if (model == null)
                return BadRequest("Request body cannot be null.");

            var response = await AdminService.AddModelAsync(model, userId);
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




        [HttpPost("PullModel")]
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

                await AdminService.InstallModelAsync(request.ModelName, progress);
                return new EmptyResult();
            }
            else
            {
                var response = await AdminService.InstallModelAsync(request.ModelName);
                return response != null ? Ok(response) : StatusCode(500, "Failed to install model.");
            }
        }

        [HttpGet("Installed")]
        public async Task<IActionResult> InstalledModels([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            if (pageNumber < 1 || pageSize < 1)
                return BadRequest("Invalid pagination parameters.");

            var response = await AdminService.InstalledModelsAsync(pageNumber, pageSize);
            return response != null ? Ok(response) : StatusCode(500, "Failed to process the request.");
        }

        [HttpDelete("SoftDelete")]
        public async Task<IActionResult> SoftDeleteModel([FromQuery] string modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                return BadRequest("Model name cannot be empty.");

            var response = await AdminService.SoftDeleteAIModelAsync(modelName);
            return response != null ? Ok(response) : StatusCode(500, "Failed to process the request.");
        }

        [HttpDelete("Uninstall")]
        public async Task<IActionResult> UninstallModel([FromBody] RemoveModelRequest model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.ModelName))
                return BadRequest("Model name cannot be empty.");

            var response = await AdminService.UninstllModelAsync(model);
            return response != null ? Ok(response) : StatusCode(500, "Failed to process the request.");
        }

        [HttpPost("AddTags")]
        public async Task<IActionResult> AddTags([FromBody] List<string> tags)
        {
            if(tags == null || tags.Count == 0)
                return BadRequest("Tags cannot be empty.");

            var response = await AdminService.AddTags(tags);

            return response != null ? Ok(response) : StatusCode(500, "Failed to process the request.");
        }

        [HttpPost("AddTagsToModel/{modelId}")]
        public async Task<IActionResult> AddTagsToModel(string modelId, [FromBody] ICollection<AddTagToModelRequest> request)
        {
            if(request == null || request.Count == 0)
                return BadRequest("Request body cannot be empty.");
            var response = await AdminService.AddTagsToModel(modelId, request);

            return response != null ? Ok(new { Message = response }) : StatusCode(500, "Failed to process the request.");
        }

        [HttpPatch("UpdateModel")]
        public async Task<IActionResult> UpdateModel(UpdateModelRequest request)
        {
            if (request == null)
                return BadRequest("Request body cannot be null.");
            var response = await AdminService.UpdateModel(request);
            return response != null ? Ok(response) : StatusCode(500, "Failed to process the request.");
        }

        [HttpGet("Users")]
        public async Task<IActionResult> Users()
        {
            var users = await AdminService.GetAllUsers();
            return Ok(users);
        }
    }
}
