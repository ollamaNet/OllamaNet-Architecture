using AdminService.Services.TagsOperations.DTOs;

namespace AdminService.Services.TagsOperations
{
    public interface ITagsOperationsService
    {
        Task<TagResponse> GetTagByIdAsync(string id);
        Task<TagOperationResult> CreateTagAsync(CreateTagRequest request);
        Task<TagOperationResult> UpdateTagAsync(UpdateTagRequest request);
        Task<TagOperationResult> SoftDeleteTagAsync(string id);
        Task<TagOperationResult> DeleteTagAsync(string id);
    }
}
