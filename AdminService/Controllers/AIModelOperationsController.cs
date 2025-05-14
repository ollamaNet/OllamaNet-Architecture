using AdminService.Services.AIModelOperations;
using AdminService.Services.AIModelOperations.DTOs;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace AdminService.Controllers
{
    [Route("api/models")]
    [ApiController]
    public class AIModelOperationsController : ControllerBase
    {
        private readonly IAIModelOperationsService _modelService;
        private readonly IValidator<CreateModelRequest> _createModelValidator;
        private readonly IValidator<UpdateModelRequest> _updateModelValidator;
        private readonly IValidator<ModelTagOperationRequest> _tagOperationValidator;
        private readonly IValidator<SearchModelRequest> _searchModelValidator;
        private readonly ILogger<AIModelOperationsController> _logger;

        public AIModelOperationsController(
            IAIModelOperationsService modelService,
            IValidator<CreateModelRequest> createModelValidator,
            IValidator<UpdateModelRequest> updateModelValidator,
            IValidator<ModelTagOperationRequest> tagOperationValidator,
            IValidator<SearchModelRequest> searchModelValidator,
            ILogger<AIModelOperationsController> logger)
        {
            _modelService = modelService;
            _createModelValidator = createModelValidator;
            _updateModelValidator = updateModelValidator;
            _tagOperationValidator = tagOperationValidator;
            _searchModelValidator = searchModelValidator;
            _logger = logger;
        }

        // GET api/models/{modelId}
        [HttpGet("{modelId}")]
        [ProducesResponseType(typeof(AIModelResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetModel(string modelId)
        {
            try
            {
                var model = await _modelService.GetModelByIdAsync(modelId);
                
                if (model == null)
                {
                    return NotFound($"Model with ID {modelId} not found");
                }
                
                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving model {ModelId}", modelId);
                return StatusCode(500, "An error occurred while retrieving the model");
            }
        }



        // POST api/models
        [HttpPost]
        [ProducesResponseType(typeof(ModelOperationResult), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        //[Authorize] // Uncomment when authorization is set up
        public async Task<IActionResult> CreateModel([FromBody] CreateModelRequest request)
        {
            try
            {
                // Validate the request
                var validationResult = await _createModelValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
                }

                var userId = Request.Headers["X-User-Id"].ToString();
                if (userId == null)
                    return Unauthorized();
                //var userId = "2396eace-718c-4131-b18a-bbbc026990f0";


                var result = await _modelService.CreateModelAsync(request, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }
                
                return CreatedAtAction(nameof(GetModel), new { modelId = result.ModelId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating model");
                return StatusCode(500, "An error occurred while creating the model");
            }
        }






        // PUT api/models
        [HttpPut]
        [ProducesResponseType(typeof(ModelOperationResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        //[Authorize] // Uncomment when authorization is set up
        public async Task<IActionResult> UpdateModel([FromBody] UpdateModelRequest request)
        {
            try
            {
                // Validate the request
                var validationResult = await _updateModelValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
                }
                
                var result = await _modelService.UpdateModelAsync(request);
                
                if (!result.Success)
                {
                    if (result.Message.Contains("not found"))
                    {
                        return NotFound(result.Message);
                    }
                    return BadRequest(result.Message);
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating model {ModelName}", request.Name);
                return StatusCode(500, "An error occurred while updating the model");
            }
        }





        // POST api/models/tags/add
        [HttpPost("tags/add")]
        [ProducesResponseType(typeof(ModelOperationResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        //[Authorize] // Uncomment when authorization is set up
        public async Task<IActionResult> AddTagsToModel([FromBody] ModelTagOperationRequest request)
        {
            try
            {
                // Validate the request
                var validationResult = await _tagOperationValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
                }
                
                var result = await _modelService.AddTagsToModelAsync(request);
                
                if (!result.Success)
                {
                    if (result.Message.Contains("not found"))
                    {
                        return NotFound(result.Message);
                    }
                    return BadRequest(result.Message);
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding tags to model {ModelId}", request.ModelId);
                return StatusCode(500, "An error occurred while adding tags to the model");
            }
        }





        // POST api/models/tags/remove
        [HttpPost("tags/remove")]
        [ProducesResponseType(typeof(ModelOperationResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        //[Authorize] // Uncomment when authorization is set up
        public async Task<IActionResult> RemoveTagsFromModel([FromBody] ModelTagOperationRequest request)
        {
            try
            {
                // Validate the request
                var validationResult = await _tagOperationValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
                }
                
                var result = await _modelService.RemoveTagsFromModelAsync(request);
                
                if (!result.Success)
                {
                    if (result.Message.Contains("not found"))
                    {
                        return NotFound(result.Message);
                    }
                    return BadRequest(result.Message);
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing tags from model {ModelId}", request.ModelId);
                return StatusCode(500, "An error occurred while removing tags from the model");
            }
        }




        // DELETE api/models/{modelId}
        [HttpDelete("{modelId}")]
        [ProducesResponseType(typeof(ModelOperationResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        //[Authorize] // Uncomment when authorization is set up
        public async Task<IActionResult> DeleteModel(string modelId)
        {
            try
            {
                var result = await _modelService.DeleteModelAsync(modelId);
                
                if (!result.Success)
                {
                    if (result.Message.Contains("not found"))
                    {
                        return NotFound(result.Message);
                    }
                    return BadRequest(result.Message);
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting model {ModelId}", modelId);
                return StatusCode(500, "An error occurred while deleting the model");
            }
        }






        // POST api/models/{modelId}/softdelete
        [HttpDelete("{modelId}/softdelete")]
        [ProducesResponseType(typeof(ModelOperationResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        //[Authorize] // Uncomment when authorization is set up
        public async Task<IActionResult> SoftDeleteModel(string modelId)
        {
            try
            {
                var result = await _modelService.SoftDeleteModelAsync(modelId);
                
                if (!result.Success)
                {
                    if (result.Message.Contains("not found"))
                    {
                        return NotFound(result.Message);
                    }
                    return BadRequest(result.Message);
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft deleting model {ModelId}", modelId);
                return StatusCode(500, "An error occurred while soft deleting the model");
            }
        }
    }
} 