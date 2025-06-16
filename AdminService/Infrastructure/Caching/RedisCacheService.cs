using System;
using System.Threading.Tasks;
using System.Text.Json;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using AdminService.Infrastructure.Caching.Interfaces;

namespace AdminService.Infrastructure.Caching
{
    /// <summary>
    /// Implementation of ICacheManager using Redis
    /// </summary>
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly RedisCacheSettings _settings;

        /// <summary>
        /// Constructor for RedisCacheService
        /// </summary>
        /// <param name="configuration">Application configuration</param>
        /// <param name="logger">Logger</param>
        /// <param name="settings">Cache settings</param>
        public RedisCacheService(
            IConfiguration configuration,
            IOptions<RedisCacheSettings> settings,
            ILogger<RedisCacheService> logger)
        {
            _logger = logger;
            _settings = settings.Value;
            
            var redisConnectionString = configuration.GetConnectionString("Redis") ?? _settings.ConnectionString;
            if (string.IsNullOrEmpty(redisConnectionString))
            {
                _logger.LogWarning("Redis connection string is not configured. Using localhost connection.");
                _redis = ConnectionMultiplexer.Connect("localhost");
                return;
            }
            
            try
            {
                _redis = ConnectionMultiplexer.Connect(redisConnectionString);
                _logger.LogInformation("Connected to Redis");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to Redis. Using localhost fallback.");
                _redis = ConnectionMultiplexer.Connect("localhost");
            }
        }

        /// <inheritdoc />
        public async Task<bool> IsConnectionAvailableAsync()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var pingResult = await _redis.GetDatabase().PingAsync();
                stopwatch.Stop();
                
                _logger.LogDebug("Redis ping completed in {ElapsedMilliseconds}ms with result {PingResult}", 
                    stopwatch.ElapsedMilliseconds, pingResult.TotalMilliseconds);
                    
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis connection check failed");
                return false;
            }
        }

        private async Task ValidateConnectionAsync()
        {
            if (!_redis.IsConnected)
            {
                _logger.LogWarning("Redis connection is not established. Attempting to verify connection.");
                if (!await IsConnectionAvailableAsync())
                {
                    throw new CacheConnectionException("Redis connection is not available");
                }
            }
        }

        private async Task ExecuteWithTimeoutAsync(Func<Task> action, string key, string operation)
        {
            var timeoutMs = _settings.OperationTimeoutMilliseconds;
            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMs));
            var timeoutTask = Task.Delay(timeoutMs, cts.Token);
            var operationTask = action();
            
            var completedTask = await Task.WhenAny(operationTask, timeoutTask);
            
            if (completedTask == timeoutTask)
            {
                cts.Cancel();
                var timeoutThreshold = TimeSpan.FromMilliseconds(timeoutMs);
                throw new CacheTimeoutException(
                    $"Cache operation {operation} timed out after {timeoutMs}ms", 
                    key,
                    timeoutThreshold);
            }
            
            cts.Cancel();
            await operationTask; // Allow any exceptions to propagate
        }

        /// <inheritdoc />
        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                await ValidateConnectionAsync();
                
                var stopwatch = Stopwatch.StartNew();
                var db = _redis.GetDatabase();
                
                RedisValue value = default;
                await ExecuteWithTimeoutAsync(async () => {
                    value = await db.StringGetAsync(key);
                }, key, "GET");
                
                stopwatch.Stop();
                _logger.LogDebug("Cache GET operation for key {Key} completed in {ElapsedMilliseconds}ms", 
                    key, stopwatch.ElapsedMilliseconds);
                
                if (!value.HasValue)
                {
                    _logger.LogDebug("Cache miss for key: {Key}", key);
                    return default;
                }

                _logger.LogDebug("Cache hit for key: {Key}", key);
                
                try 
                {
                    return JsonSerializer.Deserialize<T>(value!);
                }
                catch (JsonException ex)
                {
                    throw new CacheSerializationException($"Failed to deserialize value for key '{key}'", key, ex);
                }
            }
            catch (RedisConnectionException ex)
            {
                throw new CacheConnectionException($"Redis connection error while retrieving key '{key}'", key, ex);
            }
            catch (Exception ex) when (
                ex is not CacheException && 
                ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error getting value from cache for key: {Key}", key);
                throw new CacheOperationException("GET", key, ex);
            }
        }

        /// <inheritdoc />
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                await ValidateConnectionAsync();
                
                var stopwatch = Stopwatch.StartNew();
                var db = _redis.GetDatabase();
                
                string serializedValue;
                try
                {
                    serializedValue = JsonSerializer.Serialize(value);
                }
                catch (JsonException ex)
                {
                    throw new CacheSerializationException($"Failed to serialize value for key '{key}'", key, ex);
                }
                
                await ExecuteWithTimeoutAsync(async () => {
                    await db.StringSetAsync(key, serializedValue, expiration);
                }, key, "SET");
                
                stopwatch.Stop();
                _logger.LogDebug("Cache SET operation for key {Key} completed in {ElapsedMilliseconds}ms with expiration {Expiration}", 
                    key, stopwatch.ElapsedMilliseconds, expiration);
            }
            catch (RedisConnectionException ex)
            {
                throw new CacheConnectionException($"Redis connection error while setting key '{key}'", key, ex);
            }
            catch (Exception ex) when (
                ex is not CacheException && 
                ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error setting value in cache for key: {Key}", key);
                throw new CacheOperationException("SET", key, ex);
            }
        }

        /// <inheritdoc />
        public async Task RemoveAsync(string key)
        {
            try
            {
                await ValidateConnectionAsync();
                
                var stopwatch = Stopwatch.StartNew();
                var db = _redis.GetDatabase();
                
                await ExecuteWithTimeoutAsync(async () => {
                    await db.KeyDeleteAsync(key);
                }, key, "REMOVE");
                
                stopwatch.Stop();
                _logger.LogDebug("Cache REMOVE operation for key {Key} completed in {ElapsedMilliseconds}ms", 
                    key, stopwatch.ElapsedMilliseconds);
            }
            catch (RedisConnectionException ex)
            {
                throw new CacheConnectionException($"Redis connection error while removing key '{key}'", key, ex);
            }
            catch (Exception ex) when (
                ex is not CacheException && 
                ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error removing value from cache for key: {Key}", key);
                throw new CacheOperationException("REMOVE", key, ex);
            }
        }

        /// <inheritdoc />
        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                await ValidateConnectionAsync();
                
                var stopwatch = Stopwatch.StartNew();
                var db = _redis.GetDatabase();
                
                bool exists = false;
                await ExecuteWithTimeoutAsync(async () => {
                    exists = await db.KeyExistsAsync(key);
                }, key, "EXISTS");
                
                stopwatch.Stop();
                _logger.LogDebug("Cache EXISTS operation for key {Key} completed in {ElapsedMilliseconds}ms. Result: {Exists}", 
                    key, stopwatch.ElapsedMilliseconds, exists);
                    
                return exists;
            }
            catch (RedisConnectionException ex)
            {
                throw new CacheConnectionException($"Redis connection error while checking existence of key '{key}'", key, ex);
            }
            catch (Exception ex) when (
                ex is not CacheException && 
                ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error checking existence of key: {Key}", key);
                throw new CacheOperationException("EXISTS", key, ex);
            }
        }
    }
} 