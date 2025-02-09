namespace Ollama_Component.Services.AdminServices.Models
{
    public class RemoveModelRequest
    {
        public string ModelName { get; set; }
        public bool DeleteFromDB { get; set; }

    }
}