using AdminService.Services.TagsOperations;
using AdminService.Services.TagsOperations.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminService.Controllers
{
    [ApiController]
    [Route("api/Admin/[controller]")]
    //[Authorize]
    public class TagOperationsController : ControllerBase
    {
        private readonly ITagsOperationsService _tagsService;
        private readonly ILogger<TagOperationsController> _logger;

        public TagOperationsController(ITagsOperationsService tagsService, ILogger<TagOperationsController> logger)
        {
            _tagsService = tagsService ?? throw new ArgumentNullException(nameof(tagsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }





        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TagResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTagById(string id)
        {
            try
            {
                var tag = await _tagsService.GetTagByIdAsync(id);
                if (tag == null)
                {
                    return NotFound($"Tag with ID {id} not found");
                }

                return Ok(tag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tag with ID {TagId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the tag");
            }
        }



        [HttpPost]
        [ProducesResponseType(typeof(TagOperationResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(TagOperationResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateTag([FromBody] CreateTagRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _tagsService.CreateTagAsync(request);
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return CreatedAtAction(nameof(GetTagById), new { id = result.TagId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tag");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the tag");
            }
        }




        [HttpPut]
        [ProducesResponseType(typeof(TagOperationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(TagOperationResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(TagOperationResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateTag([FromBody] UpdateTagRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _tagsService.UpdateTagAsync(request);
                if (!result.Success)
                {
                    if (result.Message.Contains("not found"))
                    {
                        return NotFound(result);
                    }
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tag with ID {TagId}", request.Id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the tag");
            }
        }



        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(TagOperationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(TagOperationResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteTag(string id)
        {
            try
            {
                var result = await _tagsService.DeleteTagAsync(id);
                if (!result.Success)
                {
                    if (result.Message.Contains("not found"))
                    {
                        return NotFound(result);
                    }
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tag with ID {TagId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the tag");
            }
        }
    }
} 