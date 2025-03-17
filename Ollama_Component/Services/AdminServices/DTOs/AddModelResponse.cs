namespace Ollama_Component.Services.AdminServices.Models
{
    public class AddModelResponse
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string Size { get; set; }
        public string Digest { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ReleasedAt { get; set; } 

    }
}