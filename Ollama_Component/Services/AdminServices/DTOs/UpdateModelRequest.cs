namespace Ollama_Component.Services.AdminServices.DTOs
{
    public class UpdateModelRequest
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime? ReleasedAt { get; set; }
        public string? License { get; set; }
        public string? Template { get; set; }
        public string? ModelFile { get; set; }
        public string? ReferenceLink { get; set; }    }
}
