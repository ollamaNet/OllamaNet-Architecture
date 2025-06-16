namespace AdminService.Infrastructure.Caching
{
    /// <summary>
    /// Settings for Redis cache
    /// </summary>
    public class RedisCacheSettings
    {
        /// <summary>
        /// Redis connection string
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;
        
        /// <summary>
        /// Redis instance name
        /// </summary>
        public string InstanceName { get; set; } = string.Empty;
        
        /// <summary>
        /// Default cache expiration in minutes
        /// </summary>
        public int DefaultExpirationMinutes { get; set; } = 60;
        
        /// <summary>
        /// Operation timeout in milliseconds
        /// </summary>
        public int OperationTimeoutMilliseconds { get; set; } = 1000;
        
        /// <summary>
        /// Maximum number of retry attempts
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;
        
        /// <summary>
        /// Initial retry delay in milliseconds
        /// </summary>
        public int RetryDelayMilliseconds { get; set; } = 100;
        
        /// <summary>
        /// Multiplier for retry delay (for exponential backoff)
        /// </summary>
        public int RetryDelayMultiplier { get; set; } = 5;
    }
} 