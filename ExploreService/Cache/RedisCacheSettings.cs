namespace ExploreService.Cache;

public class RedisCacheSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string InstanceName { get; set; } = string.Empty;
    public int DefaultExpirationMinutes { get; set; }
    public int ModelInfoExpirationMinutes { get; set; }
    public int TagExpirationMinutes { get; set; }
    public int SearchExpirationMinutes { get; set; }
} 