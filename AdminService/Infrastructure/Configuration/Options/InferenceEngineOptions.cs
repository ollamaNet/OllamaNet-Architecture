namespace AdminService.Infrastructure.Configuration.Options
{
    /// <summary>
    /// Configuration options for the Inference Engine
    /// </summary>
    public class InferenceEngineOptions
    {
        /// <summary>
        /// The base URL for the Inference Engine API
        /// </summary>
        public string BaseUrl { get; set; } = "http://localhost:11434";
    }
} 