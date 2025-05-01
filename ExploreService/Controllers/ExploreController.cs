using ExploreService.DTOs;
using ExploreService.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ollama_DB_layer.DataBaseHelpers;
using Ollama_DB_layer.DTOs;
using System.Net;

namespace ExploreService.Controllers
{
    [ApiController]
    [Route("api/v1/explore")]
    public class ExploreController : ControllerBase
    {
        private readonly IExploreService _exploreService;
        private readonly ILogger<ExploreController> _logger;

        public ExploreController(IExploreService exploreService, ILogger<ExploreController> logger)
        {
            _exploreService = exploreService;
            _logger = logger;
        }

        /// <summary>
        /// Get paginated list of available models
        /// </summary>
        [HttpGet("models")]
        [ProducesResponseType(typeof(PagedResult<ModelCard>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetModels([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _exploreService.AvailableModels(page, pageSize);
                return Ok(result);
            }
            catch (ExploreServiceException ex)
            {
                _logger.LogError(ex, "Error getting models. Page: {Page}, PageSize: {PageSize}", page, pageSize);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get detailed information about a specific model
        /// </summary>
        [HttpGet("models/{id}")]
        [ProducesResponseType(typeof(ModelInfoResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetModelById(string id)
        {
            try
            {
                var result = await _exploreService.ModelInfo(id);
                return Ok(result);
            }
            catch (ModelNotFoundException ex)
            {
                _logger.LogWarning("Model not found: {ModelId}", ex.ModelId);
                return NotFound(new { message = ex.Message });
            }
            catch (ExploreServiceException ex)
            {
                _logger.LogError(ex, "Error getting model: {ModelId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get all available tags
        /// </summary>
        [HttpGet("tags")]
        [ProducesResponseType(typeof(List<GetTagsResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetTags()
        {
            try
            {
                var result = await _exploreService.GetTags();
                return Ok(result);
            }
            catch (ExploreServiceException ex)
            {
                _logger.LogError(ex, "Error getting tags");
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get all models associated with a specific tag
        /// </summary>
        [HttpGet("tags/{tagId}/models")]
        [ProducesResponseType(typeof(IEnumerable<ModelCard>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetModelsByTag(string tagId)
        {
            try
            {
                var result = await _exploreService.GetTagModels(tagId);
                return Ok(result);
            }
            catch (TagNotFoundException ex)
            {
                _logger.LogWarning("Tag not found: {TagId}", ex.TagId);
                return NotFound(new { message = ex.Message });
            }
            catch (ExploreServiceException ex)
            {
                _logger.LogError(ex, "Error getting models for tag: {TagId}", tagId);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = ex.Message });
            }
        }
    }
} 