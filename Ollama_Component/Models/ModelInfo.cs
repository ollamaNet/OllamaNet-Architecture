namespace Ollama_Component.Models
{
    public class ModelInfo
    {
        public string ModelId{ get; set; }

        public string Name{ get; set; }

        public string Description { get; set; }

        public string Type{ get; set; }

        public string ReleasedAT { get; set; }

        public List<string> Tags{ get; set; }
    }
}
