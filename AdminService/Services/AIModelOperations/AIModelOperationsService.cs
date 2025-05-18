using AdminService.Connectors;
using AdminService.Services.AIModelOperations.DTOs;
using Microsoft.Extensions.Logging;
using Ollama_DB_layer.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CreateModelRequest = AdminService.Services.AIModelOperations.DTOs.CreateModelRequest;
using AdminService.Services.AIModelOperations.Exceptions;
using Ollama_DB_layer.UOW;
using System.Linq;
using AdminService.Services.AIModelOperations.Mappers;

namespace AdminService.Services.AIModelOperations
{
    public class AIModelOperationsService : IAIModelOperationsService
    {
        private readonly ILogger<AIModelOperationsService> _logger;
        private readonly IOllamaConnector _ollamaConnector;
        private readonly IUnitOfWork _unitOfWork;

        public AIModelOperationsService(
            ILogger<AIModelOperationsService> logger,
            IOllamaConnector ollamaConnector,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _ollamaConnector = ollamaConnector;
            _unitOfWork = unitOfWork;
        }


        public async Task<AIModelResponse> GetModelByIdAsync(string modelId)
        {
            _logger.LogInformation($"Getting model with ID {modelId}");

            var model = await _unitOfWork.AIModelRepo.GetByIdAsync(modelId);
            
            if (model == null)
            {
                _logger.LogWarning($"Model with ID {modelId} not found");
                throw new ModelNotFoundException(modelId);
            }
            
            var mappedModel = AIModelMapper.ToResponseDto(model);

            return mappedModel;
        }




        public async Task<ModelOperationResult> CreateModelAsync(CreateModelRequest request, string userId)
        {
            _logger.LogInformation($"Creating model with name {request.Name}");

            //// 1. Check for duplicate model name
            //var existingModel = await _unitOfWork.AIModelRepo.GetByIdAsync(request.Name);
            //if (existingModel != null)
            //{
            //    _logger.LogWarning($"Model with name {request.Name} already exists");
            //    throw new ModelAlreadyExistsException(request.Name);
            //}

            AIModel modelToSave;

            if (request.FromOllama)
            {
                _logger.LogInformation("Using Ollama API to get model details");
                try
                {
                    var ollamaModelInfo = await _ollamaConnector.GetModelInfo(request.Name);
                    modelToSave = AIModelMapper.FromOllamaToAIModel(request, ollamaModelInfo, userId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get model info from Ollama API");
                    throw new ExternalModelFetchException(request.Name, ex.Message);
                }
            }
            else
            {
                _logger.LogInformation("Using direct mapping for model creation");
                modelToSave = AIModelMapper.FromRequestToAIModel(request, userId);
            }

            // 3. Assign tags if present
            if (request.Tags != null && request.Tags.Any())
            {
                modelToSave.ModelTags = request.Tags.Select(t => new ModelTag
                {
                    Tag_Id = t.TagId
                }).ToList();
            }

            // 4. Save to database
            await _unitOfWork.AIModelRepo.AddAsync(modelToSave);
            await _unitOfWork.SaveChangesAsync();

            // 5. Return value
            if (request.FromOllama)
            {
                var response = AIModelMapper.ToResponseDto(modelToSave);
                return ModelOperationResult.CreateSuccess(response.Name, $"Model {response.Name} created successfully from Ollama API");
            }
            else
            {
                return ModelOperationResult.CreateSuccess(
                    modelToSave.Name,
                    $"Model {modelToSave.Name} created successfully"
                );
            }
        }




        public async Task<ModelOperationResult> UpdateModelAsync(UpdateModelRequest request)
        {
            _logger.LogInformation($"Updating model with name {request.Name}");
            
            // 1. Fetch the model by name (ID)
            var existingModel = await _unitOfWork.AIModelRepo.GetByIdAsync(request.Name);
            if (existingModel == null)
            {
                _logger.LogWarning($"Model with name {request.Name} not found");
                throw new ModelNotFoundException(request.Name);
            }
            
            // 2. If name is being changed, check for duplicates
            if (request.Name!= null && request.Name != request.Name)
            {
                var duplicateModel = await _unitOfWork.AIModelRepo.GetByIdAsync(request.Name);
                if (duplicateModel != null)
                {
                    _logger.LogWarning($"Model with name {request.Name} already exists");
                    throw new ModelAlreadyExistsException(request.Name);
                }
            }
            
            // 3. Use the mapper to update only non-null properties
            existingModel = existingModel.UpdateFromRequest(request);
            
            // 4. Save changes to the database
            await _unitOfWork.AIModelRepo.UpdateAsync(existingModel);
            await _unitOfWork.SaveChangesAsync();
            
            // 5. Return a success result
            return ModelOperationResult.CreateSuccess(
                existingModel.Name, 
                $"Model {existingModel.Name} updated successfully"
            );
        }




        public async Task<ModelOperationResult> AddTagsToModelAsync(ModelTagOperationRequest request)
        {
            _logger.LogInformation($"Adding tags {string.Join(", ", request.TagIds)} to model {request.ModelId}");
            
            // 1. Fetch the model by ID
            var model = await _unitOfWork.AIModelRepo.GetByIdAsync(request.ModelId);
            if (model == null)
            {
                _logger.LogWarning($"Model with ID {request.ModelId} not found");
                throw new ModelNotFoundException(request.ModelId);
            }
            


            // Initialize tracking lists
            var addedTags = new List<string>();
            var nonExistentTags = new List<string>();
            var alreadyAssignedTags = new List<string>();
            


            // 2. Process each tag
            foreach (var tagId in request.TagIds)
            {
                // Check if tag exists
                var tag = await _unitOfWork.TagRepo.GetByIdAsync(tagId);
                if (tag == null)
                {
                    _logger.LogWarning($"Tag with ID {tagId} not found");
                    nonExistentTags.Add(tagId);
                    continue;
                }
                
                // Check if tag is already assigned to the model
                bool tagAlreadyAssigned = model.ModelTags != null && 
                    model.ModelTags.Any(mt => mt.Tag_Id == tagId);
                    
                if (tagAlreadyAssigned)
                {
                    _logger.LogInformation($"Tag {tagId} is already assigned to model {request.ModelId}");
                    alreadyAssignedTags.Add(tagId);
                    continue;
                }
                
                // Add tag to model
                if (model.ModelTags == null)
                {
                    model.ModelTags = new List<ModelTag>();
                }
                
                model.ModelTags.Add(new ModelTag
                {
                    Tag_Id = tagId
                });
                
                addedTags.Add(tagId);
            }
            
            // 3. Save changes if any tags were added
            if (addedTags.Any())
            {
                await _unitOfWork.AIModelRepo.UpdateAsync(model);
                await _unitOfWork.SaveChangesAsync();
            }
            
            // 4. Build result message
            string resultMessage = BuildTagOperationResultMessage(addedTags, nonExistentTags, alreadyAssignedTags, "added to");
            
            // 5. Return result
            return ModelOperationResult.CreateSuccess(
                request.ModelId,
                resultMessage
            );
        }
        
        // Helper method to build tag operation result messages
        private string BuildTagOperationResultMessage(
            List<string> processedTags, 
            List<string> nonExistentTags, 
            List<string> skippedTags,
            string operation)
        {
            var messageParts = new List<string>();
            
            if (processedTags.Any())
            {
                messageParts.Add($"{processedTags.Count} tag(s) successfully {operation} model: {string.Join(", ", processedTags)}");
            }
            
            if (nonExistentTags.Any())
            {
                messageParts.Add($"{nonExistentTags.Count} tag(s) not found: {string.Join(", ", nonExistentTags)}");
            }
            
            if (skippedTags.Any())
            {
                messageParts.Add($"{skippedTags.Count} tag(s) skipped: {string.Join(", ", skippedTags)}");
            }
            
            return string.Join(". ", messageParts);
        }




        public async Task<ModelOperationResult> RemoveTagsFromModelAsync(ModelTagOperationRequest request)
        {
            _logger.LogInformation($"Removing tags {string.Join(", ", request.TagIds)} from model {request.ModelId}");
            
            // 1. Fetch the model by ID
            var model = await _unitOfWork.AIModelRepo.GetByIdAsync(request.ModelId);
            if (model == null)
            {
                _logger.LogWarning($"Model with ID {request.ModelId} not found");
                throw new ModelNotFoundException(request.ModelId);
            }
            



            // tracking lists
            var removedTags = new List<string>();
            var notAssignedTags = new List<string>();
            
            // 2. Process each tag
            if (model.ModelTags != null)
            {
                foreach (var tagId in request.TagIds)
                {
                    // Check if tag is assigned to the model
                    var tagToRemove = model.ModelTags.FirstOrDefault(mt => mt.Tag_Id == tagId);
                    
                    if (tagToRemove == null)
                    {
                        _logger.LogInformation($"Tag {tagId} is not assigned to model {request.ModelId}");
                        notAssignedTags.Add(tagId);
                        continue;
                    }
                    
                    // Remove tag from model
                    model.ModelTags.Remove(tagToRemove);
                    removedTags.Add(tagId);
                }
            }
            else
            {
                // If model has no tags, all requested removals are technically "not assigned"
                notAssignedTags.AddRange(request.TagIds);
            }
            
            // 3. Save changes if any tags were removed
            if (removedTags.Any())
            {
                await _unitOfWork.AIModelRepo.UpdateAsync(model);
                await _unitOfWork.SaveChangesAsync();
            }
            
            // 4. Build result message
            string resultMessage = BuildTagOperationResultMessage(removedTags, new List<string>(), notAssignedTags, "removed from");
            
            // 5. Return result
            return ModelOperationResult.CreateSuccess(
                request.ModelId,
                resultMessage
            );
        }




        public async Task<ModelOperationResult> DeleteModelAsync(string modelId)
        {
            _logger.LogInformation($"Deleting model with ID {modelId}");
            
            // 1. Fetch the model by ID
            var model = await _unitOfWork.AIModelRepo.GetByIdAsync(modelId);
            if (model == null)
            {
                _logger.LogWarning($"Model with ID {modelId} not found");
                throw new ModelNotFoundException(modelId) ;
            }
            
            // 2. Remove the model from the database (hard delete)
            await _unitOfWork.AIModelRepo.DeleteAsync(modelId);
            await _unitOfWork.SaveChangesAsync();
            
            // 3. Return a success result
            return ModelOperationResult.CreateSuccess(
                modelId,
                $"Model {modelId} deleted successfully"
            );
        }



        public async Task<ModelOperationResult> SoftDeleteModelAsync(string modelId)
        {
            _logger.LogInformation($"Soft deleting model with ID {modelId}");
            
            // 1. Fetch the model by ID
            var model = await _unitOfWork.AIModelRepo.GetByIdAsync(modelId);
            if (model == null)
            {
                _logger.LogWarning($"Model with ID {modelId} not found");
                throw new ModelNotFoundException(modelId);
            }
            
            // 2. Set IsDeleted = true
            model.IsDeleted = true;
            
            // 3. Save changes
            await _unitOfWork.AIModelRepo.UpdateAsync(model);
            await _unitOfWork.SaveChangesAsync();
            
            // 4. Log the soft delete operation
            _logger.LogInformation($"Model with ID {modelId} was soft deleted");
            
            // 5. Return a success result
            return ModelOperationResult.CreateSuccess(
                modelId,
                $"Model {modelId} soft deleted successfully"
            );
        }
    }
}