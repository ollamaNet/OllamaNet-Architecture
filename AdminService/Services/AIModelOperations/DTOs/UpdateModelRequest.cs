namespace AdminService.Services.AIModelOperations.DTOs
{
    public class UpdateModelRequest
    {
        public string Name { get; set; } = string.Empty; // Required for identification
        
        public string? Description { get; set; }
        
        public string? Version { get; set; }
        
        public string? Size { get; set; }
        
        public DateTime? ReleasedAt { get; set; }
        
        public string? License { get; set; }
        
        public string? Template { get; set; }
        
        public string? ModelFile { get; set; }
        
        public string? ReferenceLink { get; set; }
        
        public string? ImageUrl { get; set; }
        
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
        
        public string? SizeLabel { get; set; }
        
        public string? ModelType { get; set; }
    }
}