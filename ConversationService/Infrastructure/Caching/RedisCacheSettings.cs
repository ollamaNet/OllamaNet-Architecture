namespace ConversationServices.Infrastructure.Caching;

public class RedisCacheSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string InstanceName { get; set; } = string.Empty;
    public int DefaultExpirationMinutes { get; set; }
    public int ModelInfoExpirationMinutes { get; set; }
    public int TagExpirationMinutes { get; set; }
    public int SearchExpirationMinutes { get; set; }
    
    // Timeout settings
    public int OperationTimeoutMilliseconds { get; set; } = 1000;
    
    // Retry settings
    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryDelayMilliseconds { get; set; } = 100;
    public int RetryDelayMultiplier { get; set; } = 5;
} 