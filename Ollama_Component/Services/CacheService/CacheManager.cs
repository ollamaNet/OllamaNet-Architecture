using Microsoft.Extensions.Caching.Memory;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Ollama_Component.Services.CacheService
{
    public class CacheManager
    {
        private readonly IMemoryCache _cache;

        public CacheManager(IMemoryCache cache)
        {
            _cache = cache;
        }

        public bool TryGetChatHistory(string cacheKey, out ChatHistory chatHistory)
        {
            return _cache.TryGetValue(cacheKey, out chatHistory);
        }

        public void SetChatHistory(string cacheKey, ChatHistory chatHistory)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(200))
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                .SetPriority(CacheItemPriority.High);

            _cache.Set(cacheKey, chatHistory, cacheEntryOptions);
        }
    }
}
