using Microsoft.Extensions.Caching.Memory;

namespace ProxyCacheServer
{
    public class GenericProxyCache<T> where T : IProxyCacheItem, new()
    {
        private readonly IMemoryCache _cache;

        public GenericProxyCache(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<T> GetAsync(string CacheItemName, params string[] args)
        {
            if (!_cache.TryGetValue(CacheItemName, out T value))
            {
                value = new T();
                await value.FillFromWebAsync(args);
                _cache.Set(CacheItemName, value);
            }

            return value;
        }

        public async Task<T> GetAsync(string CacheItemName, double dt_seconds, params string[] args)
        {
            if (!_cache.TryGetValue(CacheItemName, out T value))
            {
                value = new T();
                await value.FillFromWebAsync(args);
                _cache.Set(CacheItemName, value, TimeSpan.FromSeconds(dt_seconds));
            }

            return value;
        }

        public async Task<T> GetAsync(string CacheItemName, DateTimeOffset dt, params string[] args)
        {
            if (!_cache.TryGetValue(CacheItemName, out T value))
            {
                value = new T();
                await value.FillFromWebAsync(args);
                _cache.Set(CacheItemName, value, dt);
            }

            return value;
        }
    }
}
