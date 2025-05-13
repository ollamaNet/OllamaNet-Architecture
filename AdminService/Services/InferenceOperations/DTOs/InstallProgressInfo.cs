namespace AdminService.Services.InferenceOperations.DTOs
{
    public class InstallProgressInfo
    { 
        public double Progress { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Digest { get; set; } = string.Empty;
        public long Total { get; set; }
        public long Completed { get; set; }
    }
}
