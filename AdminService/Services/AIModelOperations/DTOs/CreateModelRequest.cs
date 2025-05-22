using System.ComponentModel.DataAnnotations;

namespace AdminService.Services.AIModelOperations.DTOs
{
    public class CreateModelRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public string Version { get; set; } = string.Empty;
        
        public string Size { get; set; } = string.Empty;
        
        public DateTime ReleasedAt { get; set; }
        
        public string License { get; set; } = string.Empty;
        
        public string? Template { get; set; }
        
        public string? ModelFile { get; set; }
        
        public string? ReferenceLink { get; set; }
        
        public string? ImageUrl { get; set; }
        
        public bool FromOllama { get; set; } = false;
        
        public string? Digest { get; set; }
        
        public string? Format { get; set; }
        
        public string? ParameterSize { get; set; }
        
        public string? QuantizationLevel { get; set; }
        
        public string? ParentModel { get; set; }
        
        public string? Family { get; set; }
        
        public List<string>? Families { get; set; }
        
        public List<string>? Languages { get; set; }
        
        public string? Architecture { get; set; }
        
        public int? FileType { get; set; }
        
        public long? ParameterCount { get; set; }
        
        public int? QuantizationVersion { get; set; }
        
        [MaxLength(50)]
        public string? SizeLabel { get; set; }
        
        [MaxLength(50)]
        public string? ModelType { get; set; }
        
        public List<AddTagRequest>? Tags { get; set; }
    }
    
    public class AddTagRequest
    {
        [Required]
        public string TagId { get; set; } = string.Empty;
    }
} 