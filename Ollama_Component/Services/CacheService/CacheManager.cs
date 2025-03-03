using Microsoft.SemanticKernel.ChatCompletion;
using StackExchange.Redis;
using System;
using System.Text.Json;

namespace Ollama_Component.Services.CacheService
{
    public class CacheManager
    {
        private readonly IDatabase _redisDb;

        public CacheManager(IConnectionMultiplexer redis)
        {
            _redisDb = redis.GetDatabase();
        }

        public bool TryGetChatHistory(string cacheKey, out ChatHistory chatHistory)
        {
            try
            {
                var cachedValue = _redisDb.StringGet(cacheKey);
                if (cachedValue.HasValue)
                {
                    chatHistory = JsonSerializer.Deserialize<ChatHistory>(cachedValue);
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log the error (you can use a logging framework like Serilog or NLog)
                Console.Error.WriteLine($"Error retrieving chat history from Redis: {ex.Message}");
            }

            chatHistory = null;
            return false;
        }

        public void SetChatHistory(string cacheKey, ChatHistory chatHistory)
        {
            try
            {
                var serializedValue = JsonSerializer.Serialize(chatHistory);
                var cacheEntryOptions = new TimeSpan(0, 30, 0); // 30 minutes sliding expiration
                var absoluteExpiration = DateTime.UtcNow.AddHours(24); // 24 hours absolute expiration

                _redisDb.StringSet(cacheKey, serializedValue, absoluteExpiration - DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                // Log the error
                Console.Error.WriteLine($"Error setting chat history in Redis: {ex.Message}");
            }
        }
    }
}