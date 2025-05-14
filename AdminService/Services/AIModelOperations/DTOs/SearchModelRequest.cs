namespace AdminService.Services.AIModelOperations.DTOs
{
    public class SearchModelRequest
    {
        public string? SearchTerm { get; set; }
        
        public List<string>? TagIds { get; set; }
        
        public bool? ActiveOnly { get; set; }
        
        public string? OwnerId { get; set; }
        
        public int PageNumber { get; set; } = 1;
        
        public int PageSize { get; set; } = 10;
    }
} 