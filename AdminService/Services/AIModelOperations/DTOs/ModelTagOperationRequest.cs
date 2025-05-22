using System.ComponentModel.DataAnnotations;

namespace AdminService.Services.AIModelOperations.DTOs
{
    public class ModelTagOperationRequest
    {
        [Required]
        public string ModelId { get; set; } = string.Empty;
        
        [Required]
        public List<string> TagIds { get; set; } = new List<string>();
    }
} 