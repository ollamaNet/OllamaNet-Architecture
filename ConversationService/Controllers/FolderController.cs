using ConversationService.FolderService;
using ConversationService.FolderService.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ConversationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FolderController : ControllerBase
    {
        private readonly IFolderService _folderService;
        private readonly IValidator<CreateFolderRequest> _createFolderValidator;
        private readonly IValidator<UpdateFolderRequest> _updateFolderValidator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FolderController(
            IFolderService folderService,
            IValidator<CreateFolderRequest> createFolderValidator,
            IValidator<UpdateFolderRequest> updateFolderValidator,
            IHttpContextAccessor httpContextAccessor)
        {
            _folderService = folderService ?? throw new ArgumentNullException(nameof(folderService));
            _createFolderValidator = createFolderValidator ?? throw new ArgumentNullException(nameof(createFolderValidator));
            _updateFolderValidator = updateFolderValidator ?? throw new ArgumentNullException(nameof(updateFolderValidator));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <summary>
        /// Creates a new folder
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateFolderRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    error = "Invalid request model",
                    details = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                });
            }

            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue("UserId");
            if (userId == null)
                return Unauthorized(new { error = "User not authenticated" });

            var validationResult = await _createFolderValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(new { error = "Validation failed", details = validationResult.Errors });

            try
            {
                var response = await _folderService.CreateFolderAsync(userId, request);
                return CreatedAtAction(nameof(GetById), new { folderId = response.Id }, response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a folder permanently
        /// </summary>
        [HttpDelete("{folderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(string folderId)
        {
            if (string.IsNullOrEmpty(folderId))
                return BadRequest(new { error = "Folder ID is required" });

            try
            {
                var result = await _folderService.DeleteFolderAsync(folderId);
                if (!result)
                    return NotFound(new { error = "Folder not found" });

                return Ok(new { message = "Folder deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Soft deletes a folder
        /// </summary>
        [HttpDelete("{folderId}/soft")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SoftDelete(string folderId)
        {
            if (string.IsNullOrEmpty(folderId))
                return BadRequest(new { error = "Folder ID is required" });

            try
            {
                var result = await _folderService.SoftDeleteFolderAsync(folderId);
                if (!result)
                    return NotFound(new { error = "Folder not found" });

                return Ok(new { message = "Folder soft deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Updates a folder's name
        /// </summary>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update([FromBody] UpdateFolderRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    error = "Invalid request model",
                    details = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                });
            }

            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue("UserId");
            if (userId == null)
                return Unauthorized(new { error = "User not authenticated" });

            var validationResult = await _updateFolderValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(new { error = "Validation failed", details = validationResult.Errors });

            try
            {
                var result = await _folderService.UpdateFolderAsync(userId, request);
                if (!result)
                    return NotFound(new { error = "Folder not found or does not belong to user" });

                return Ok(new { message = "Folder updated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets all folders and conversations for the current user
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue("UserId");
            if (userId == null)
                return Unauthorized(new { error = "User not authenticated" });

            try
            {
                var folderStructure = await _folderService.GetRootFolderStructureAsync(userId);
                if (folderStructure == null)
                    return NotFound(new { error = "No folders found for the user" });

                return Ok(folderStructure);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets a specific folder by ID
        /// </summary>
        [HttpGet("{folderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(string folderId)
        {
            if (string.IsNullOrEmpty(folderId))
                return BadRequest(new { error = "Folder ID is required" });

            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue("UserId");
            if (userId == null)
                return Unauthorized(new { error = "User not authenticated" });

            try
            {
                var folder = await _folderService.GetAllMainAsync(userId, folderId);
                if (folder == null)
                    return NotFound(new { error = "Folder not found or does not belong to user" });

                return Ok(folder);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
    }
}
    