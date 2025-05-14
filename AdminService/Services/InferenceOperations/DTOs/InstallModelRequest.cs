namespace AdminService.Services.InferenceOperations.DTOs
{
    public class InstallModelRequest
    {
        public string ModelName{ get; set; }
        public bool Stream{ get; set; }
    }
}