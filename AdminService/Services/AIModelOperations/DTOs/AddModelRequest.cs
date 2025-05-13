namespace AdminService.Services.AIModelOperations.DTOs
{
    // This class mirrors the original AdminService.DTOs.AddModelRequest
    // It's used for internal mapping between service DTOs and domain models
    public class AddModelRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public DateTime ReleasedAt { get; set; }
        public string? ReferenceLink { get; set; }
        public string? ImageUrl { get; set; }
        public bool FromOllama { get; set; }
        public string? Digest { get; set; }
        public string? Format { get; set; }
        public string? ParameterSize { get; set; }
        public string? QuantizationLevel { get; set; }
        public string? License { get; set; }
        public string? ModelFile { get; set; }
        public string? Template { get; set; }
        public string? ParentModel { get; set; }
        public string? Family { get; set; }
        public List<string>? Families { get; set; }
        public List<string>? Languages { get; set; }
        public string? Architecture { get; set; }
        public int FileType { get; set; }
        public long ParameterCount { get; set; }
        public int QuantizationVersion { get; set; }
        public string? SizeLabel { get; set; }
        public string? ModelType { get; set; }
    }
} 