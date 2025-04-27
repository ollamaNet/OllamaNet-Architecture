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
    [Route("api/admin")]
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

        [HttpPost("models")]
        public async Task<IActionResult> CreateModel([FromBody] AddModelRequest model)
        {
            var userId = Request.Headers["X-User-Id"].ToString();
            if (userId == null)
                return Unauthorized();

            if (model == null)
                return BadRequest("Request body cannot be null.");

            var response = await AdminService.AddModelAsync(model, userId);
            return response != null ? Ok(response) : StatusCode(500, "Failed to process the request.");
        }

        [HttpGet("models/info")]
        public async Task<IActionResult> GetModelInfo([FromQuery] string modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                return BadRequest("Model name cannot be empty.");

            var response = await AdminService.ModelInfoAsync(modelName);
            return response != null ? Ok(response) : StatusCode(500, "Failed to process the request.");
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

                await AdminService.InstallModelAsync(request.ModelName, progress);
                return new EmptyResult();
            }
            else
            {
                var response = await AdminService.InstallModelAsync(request.ModelName);
                return response != null ? Ok(response) : StatusCode(500, "Failed to install model.");
            }
        }

        [HttpGet("models")]
        public async Task<IActionResult> GetModels([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            if (pageNumber < 1 || pageSize < 1)
                return BadRequest("Invalid pagination parameters.");

            var response = await AdminService.InstalledModelsAsync(pageNumber, pageSize);
            return response != null ? Ok(response) : StatusCode(500, "Failed to process the request.");
        }

        [HttpDelete("models/soft-delete")]
        public async Task<IActionResult> SoftDeleteModel([FromQuery] string modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                return BadRequest("Model name cannot be empty.");

            var response = await AdminService.SoftDeleteAIModelAsync(modelName);
            return response != null ? Ok(response) : StatusCode(500, "Failed to process the request.");
        }

        [HttpDelete("models")]
        public async Task<IActionResult> DeleteModel([FromBody] RemoveModelRequest model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.ModelName))
                return BadRequest("Model name cannot be empty.");

            var response = await AdminService.UninstllModelAsync(model);
            return response != null ? Ok(response) : StatusCode(500, "Failed to process the request.");
        }

        [HttpPost("tags")]
        public async Task<IActionResult> CreateTags([FromBody] List<string> tags)
        {
            if(tags == null || tags.Count == 0)
                return BadRequest("Tags cannot be empty.");

            var response = await AdminService.AddTags(tags);

            return response != null ? Ok(response) : StatusCode(500, "Failed to process the request.");
        }

        [HttpPost("models/{modelId}/tags")]
        public async Task<IActionResult> AddTagsToModel(string modelId, [FromBody] ICollection<AddTagToModelRequest> request)
        {
            if(request == null || request.Count == 0)
                return BadRequest("Request body cannot be empty.");
            var response = await AdminService.AddTagsToModel(modelId, request);

            return response != null ? Ok(new { Message = response }) : StatusCode(500, "Failed to process the request.");
        }

        [HttpPatch("models")]
        public async Task<IActionResult> UpdateModel(UpdateModelRequest request)
        {
            if (request == null)
                return BadRequest("Request body cannot be null.");
            var response = await AdminService.UpdateModel(request);
            return response != null ? Ok(response) : StatusCode(500, "Failed to process the request.");
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await AdminService.GetAllUsers();
            return Ok(users);
        }
    }
}
