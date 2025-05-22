//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using FluentValidation;
//using Microsoft.AspNetCore.Authorization;
//using System.Security.Claims;
//using AdminService.DTOs;

//namespace AdminService.Controllers
//{
//    //[Authorize("Admin")]
//    [Route("api/admin")]
//    [ApiController]
//    public class AdminController : Controller
//    {
//        public IAdminService AdminService { get; set; }
//        private readonly IHttpContextAccessor _httpContextAccessor;

//        public AdminController(IAdminService adminService, IHttpContextAccessor httpContextAccessor)
//        {
//            AdminService = adminService;
//            _httpContextAccessor = httpContextAccessor;
//        }





//        [HttpPost("models")]
//        public async Task<IActionResult> CreateModel([FromBody] AddModelRequest model)
//        {
//            var userId = Request.Headers["X-User-Id"].ToString();
//            if (userId == null)
//                return Unauthorized();

//            if (model == null)
//                return BadRequest("Request body cannot be null.");

//            var response = await AdminService.AddModelAsync(model, userId);
//            return response != null ? Ok(response) : StatusCode(500, "Failed to process the request.");
//        }





//        [HttpDelete("models/soft-delete")]
//        public async Task<IActionResult> SoftDeleteModel([FromQuery] string modelName)
//        {
//            if (string.IsNullOrWhiteSpace(modelName))
//                return BadRequest("Model name cannot be empty.");

//            var response = await AdminService.SoftDeleteAIModelAsync(modelName);
//            return response != null ? Ok(response) : StatusCode(500, "Failed to process the request.");
//        }




//        [HttpPost("tags")]
//        public async Task<IActionResult> CreateTags([FromBody] List<string> tags)
//        {
//            if(tags == null || tags.Count == 0)
//                return BadRequest("Tags cannot be empty.");

//            var response = await AdminService.AddTags(tags);

//            return response != null ? Ok(response) : StatusCode(500, "Failed to process the request.");
//        }






//        [HttpPost("models/{modelId}/tags")]
//        public async Task<IActionResult> AddTagsToModel(string modelId, [FromBody] ICollection<AddTagToModelRequest> request)
//        {
//            if(request == null || request.Count == 0)
//                return BadRequest("Request body cannot be empty.");
//            var response = await AdminService.AddTagsToModel(modelId, request);

//            return response != null ? Ok(new { Message = response }) : StatusCode(500, "Failed to process the request.");
//        }





//        [HttpPatch("models")]
//        public async Task<IActionResult> UpdateModel(UpdateModelRequest request)
//        {
//            if (request == null)
//                return BadRequest("Request body cannot be null.");
//            var response = await AdminService.UpdateModel(request);
//            return response != null ? Ok(response) : StatusCode(500, "Failed to process the request.");
//        }



//    }
//}
