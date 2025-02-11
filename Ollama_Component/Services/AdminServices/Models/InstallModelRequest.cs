namespace Ollama_Component.Services.AdminServices.Models
{
    public class InstallModelRequest
    {
        public string ModelName{ get; set; }
        public bool Stream{ get; set; }
    }
}