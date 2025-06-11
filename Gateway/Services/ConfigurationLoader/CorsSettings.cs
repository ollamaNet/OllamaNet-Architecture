namespace Gateway.Configurations
{
    public class CorsSettings
    {
        public bool AllowAllOrigins { get; set; }
        public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
        public bool AllowCredentials { get; set; }
        public string[] AllowedMethods { get; set; } = Array.Empty<string>();
        public string[] AllowedHeaders { get; set; } = Array.Empty<string>();
    }
} 