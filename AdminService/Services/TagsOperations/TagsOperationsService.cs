using AdminService.Services.TagsOperations.DTOs;
using Microsoft.Extensions.Logging;
using Ollama_DB_layer.Entities;
using Ollama_DB_layer.Repositories;
using Ollama_DB_layer.UOW;

namespace AdminService.Services.TagsOperations
{
    public class TagsOperationsService : ITagsOperationsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TagsOperationsService> _logger;

        public TagsOperationsService(IUnitOfWork unitOfWork, ILogger<TagsOperationsService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TagResponse> GetTagByIdAsync(string id)
        {
            try
            {
                var tag = await _unitOfWork.TagRepo.GetByIdAsync(id);
                if (tag == null || tag.IsDeleted)
                {
                    return null;
                }

                return new TagResponse
                {
                    Id = tag.Id,
                    Name = tag.Name,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tag with ID {TagId}", id);
                throw;
            }
        }

        public async Task<TagOperationResult> CreateTagAsync(CreateTagRequest request)
        {
            try
            {
                // Check if tag with same name exists
                //var existingTag = await _unitOfWork.TagRepo.FindAsync(t => t.Name == request.Name && !t.IsDeleted);
                //if (existingTag != null)
                //{
                //    return TagOperationResult.CreateFailure($"Tag with name '{request.Name}' already exists");
                //}

                var tag = new Tag
                {
                    Name = request.Name,
                    IsDeleted = false
                };

                await _unitOfWork.TagRepo.AddAsync(tag);
                await _unitOfWork.SaveChangesAsync();

                return TagOperationResult.CreateSuccess(tag.Id, "Tag created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tag with name {TagName}", request.Name);
                return TagOperationResult.CreateFailure("An error occurred while creating the tag");
            }
        }

        public async Task<TagOperationResult> UpdateTagAsync(UpdateTagRequest request)
        {
            try
            {
                var tag = await _unitOfWork.TagRepo.GetByIdAsync(request.Id);
                if (tag == null || tag.IsDeleted)
                {
                    return TagOperationResult.CreateFailure("Tag not found", request.Id);
                }

                // Check if another tag with the new name exists
                //var existingTag = await _unitOfWork.TagRepo.FindAsync(t => 
                //    t.Name == request.Name && 
                //    t.Id != request.Id && 
                //    !t.IsDeleted);

                //if (existingTag != null)
                //{
                //    return TagOperationResult.CreateFailure($"Tag with name '{request.Name}' already exists", request.Id);
                //}

                tag.Name = request.Name;
                await _unitOfWork.SaveChangesAsync();

                return TagOperationResult.CreateSuccess(tag.Id, "Tag updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tag with ID {TagId}", request.Id);
                return TagOperationResult.CreateFailure("An error occurred while updating the tag", request.Id);
            }
        }

        public async Task<TagOperationResult> DeleteTagAsync(string id)
        {
            try
            {
                var tag = await _unitOfWork.TagRepo.GetByIdAsync(id);
                if (tag == null)
                {
                    return TagOperationResult.CreateFailure("Tag not found", id);
                }

                if (tag.IsDeleted)
                {
                    return TagOperationResult.CreateFailure("Tag is already deleted", id);
                }

                // delete
                await _unitOfWork.TagRepo.DeleteAsync(id); 
                await _unitOfWork.SaveChangesAsync();

                return TagOperationResult.CreateSuccess(tag.Id, "Tag deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tag with ID {TagId}", id);
                return TagOperationResult.CreateFailure("An error occurred while deleting the tag", id);
            }
        }

        public async Task<TagOperationResult> SoftDeleteTagAsync(string id)
        {
            try
            {
                var tag = await _unitOfWork.TagRepo.GetByIdAsync(id);
                if (tag == null)
                {
                    return TagOperationResult.CreateFailure("Tag not found", id);
                }

                if (tag.IsDeleted)
                {
                    return TagOperationResult.CreateFailure("Tag is already deleted", id);
                }

                // Soft delete
                tag.IsDeleted = true;
                await _unitOfWork.SaveChangesAsync();

                return TagOperationResult.CreateSuccess(tag.Id, "Tag deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tag with ID {TagId}", id);
                return TagOperationResult.CreateFailure("An error occurred while deleting the tag", id);
            }
        }
    }
}
