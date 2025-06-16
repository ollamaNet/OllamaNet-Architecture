namespace AdminService.Infrastructure.Configuration.Options
{
    /// <summary>
    /// Configuration options for RabbitMQ
    /// </summary>
    public class RabbitMQOptions
    {
        /// <summary>
        /// The hostname of the RabbitMQ server
        /// </summary>
        public string HostName { get; set; } = "localhost";
        
        /// <summary>
        /// The port of the RabbitMQ server
        /// </summary>
        public int Port { get; set; } = 5672;
        
        /// <summary>
        /// The username for RabbitMQ authentication
        /// </summary>
        public string UserName { get; set; } = "guest";
        
        /// <summary>
        /// The password for RabbitMQ authentication
        /// </summary>
        public string Password { get; set; } = "guest";
        
        /// <summary>
        /// The virtual host to use
        /// </summary>
        public string VirtualHost { get; set; } = "/";
        
        /// <summary>
        /// The exchange name for service discovery
        /// </summary>
        public string Exchange { get; set; } = "service-discovery";
        
        /// <summary>
        /// The queue name for inference URL updates
        /// </summary>
        public string InferenceUrlQueue { get; set; } = "inference-url-updates";
        
        /// <summary>
        /// The routing key for inference URL updates
        /// </summary>
        public string InferenceUrlRoutingKey { get; set; } = "inference.url.changed";
    }
} 