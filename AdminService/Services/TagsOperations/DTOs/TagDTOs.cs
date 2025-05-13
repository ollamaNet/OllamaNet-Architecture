using System.ComponentModel.DataAnnotations;

namespace AdminService.Services.TagsOperations.DTOs
{
    public class TagResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class CreateTagRequest
    {
        [Required(ErrorMessage = "Tag name is required")]
        [MaxLength(100, ErrorMessage = "Tag name cannot exceed 100 characters")]
        public string Name { get; set; }
    }



    public class UpdateTagRequest
    {
        [Required(ErrorMessage = "Tag ID is required")]
        public string Id { get; set; }

        [Required(ErrorMessage = "Tag name is required")]
        [MaxLength(100, ErrorMessage = "Tag name cannot exceed 100 characters")]
        public string Name { get; set; }
    }



    public class TagOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string TagId { get; set; }

        public static TagOperationResult CreateSuccess(string tagId, string message = "Operation completed successfully")
        {
            return new TagOperationResult
            {
                Success = true,
                Message = message,
                TagId = tagId
            };
        }




        public static TagOperationResult CreateFailure(string message, string tagId = null)
        {
            return new TagOperationResult
            {
                Success = false,
                Message = message,
                TagId = tagId
            };
        }
    }
} 