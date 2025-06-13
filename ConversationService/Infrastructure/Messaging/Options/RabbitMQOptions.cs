namespace ConversationService.Infrastructure.Messaging.Options;

public class RabbitMQOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string Exchange { get; set; } = "service-discovery";
    public string InferenceUrlQueue { get; set; } = "inference-url-updates";
    public string InferenceUrlRoutingKey { get; set; } = "inference.url.changed";
} 