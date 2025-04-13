using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ExploreService;
using ExploreService.DTOs;
using Microsoft.Extensions.Logging;
using System.Net;
using Ollama_DB_layer.DataBaseHelpers;
using Ollama_DB_layer.DTOs;

namespace ExploreService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ExploreController : ControllerBase
    {
        private readonly IExploreService _exploreService;
        private readonly ILogger<ExploreController> _logger;

        public ExploreController(
            IExploreService exploreService,
            ILogger<ExploreController> logger)
        {
            _exploreService = exploreService ?? throw new ArgumentNullException(nameof(exploreService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("Models")]
        [ProducesResponseType(typeof(PagedResult<ModelCard>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Models(int PageNumber, int PageSize)
        {
            try
            {
                if (PageSize <= 0 || PageNumber <= 0)
                {
                    _logger.LogWarning("Invalid pagination parameters: PageNumber={PageNumber}, PageSize={PageSize}", PageNumber, PageSize);
                    return BadRequest("PageNumber and PageSize must be greater than zero.");
                }

                var response = await _exploreService.AvailableModels(PageNumber, PageSize);
                
                if (response == null)
                {
                    _logger.LogError("Failed to retrieve models for PageNumber={PageNumber}, PageSize={PageSize}", PageNumber, PageSize);
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve models.");
                }

                // Add cache control headers
                Response.Headers.Add("X-Cache-Status", "HIT");
                Response.Headers.Add("Cache-Control", "public, max-age=300"); // 5 minutes cache

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving models for PageNumber={PageNumber}, PageSize={PageSize}", PageNumber, PageSize);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpGet("ModelInfo/{modelName}")]
        [ProducesResponseType(typeof(ModelInfoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ModelInfo(string modelName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(modelName))
                {
                    _logger.LogWarning("Empty model name provided");
                    return BadRequest("Model name cannot be empty.");
                }

                var response = await _exploreService.ModelInfo(modelName);
                
                if (response == null)
                {
                    _logger.LogWarning("Model not found: {ModelName}", modelName);
                    return NotFound($"Model '{modelName}' not found.");
                }

                // Add cache control headers
                Response.Headers.Add("X-Cache-Status", "HIT");
                Response.Headers.Add("Cache-Control", "public, max-age=3600"); // 1 hour cache

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error retrieving model info for {ModelName}", modelName);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving model info for {ModelName}", modelName);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpGet("GetTags")]
        [ProducesResponseType(typeof(List<GetTagsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTags()
        {
            try
            {
                var response = await _exploreService.GetTags();
                
                if (response == null || !response.Any())
                {
                    _logger.LogWarning("No tags found");
                    return Ok(new List<GetTagsResponse>()); // Return empty list instead of error
                }

                // Add cache control headers
                Response.Headers.Add("X-Cache-Status", "HIT");
                Response.Headers.Add("Cache-Control", "public, max-age=3600"); // 1 hour cache

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tags");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving tags.");
            }
        }

        [HttpGet("GetTagModels/{tagId}")]
        [ProducesResponseType(typeof(IEnumerable<ModelCard>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTagModels(string tagId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tagId))
                {
                    _logger.LogWarning("Empty tag ID provided");
                    return BadRequest("Tag ID cannot be empty.");
                }

                var response = await _exploreService.GetTagModels(tagId);
                
                if (response == null || !response.Any())
                {
                    _logger.LogWarning("No models found for tag: {TagId}", tagId);
                    return Ok(new List<ModelCard>()); // Return empty list instead of error
                }

                // Add cache control headers
                Response.Headers.Add("X-Cache-Status", "HIT");
                Response.Headers.Add("Cache-Control", "public, max-age=3600"); // 1 hour cache

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving models for tag {TagId}", tagId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving models for the specified tag.");
            }
        }
    }
}
