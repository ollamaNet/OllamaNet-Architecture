namespace Ollama_Component.Services.AdminServices.DTOs
{
    public class InstallModelRequest
    {
        public string ModelName{ get; set; }
        public bool Stream{ get; set; }
    }
}