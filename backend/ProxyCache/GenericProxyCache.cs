using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;


namespace ProxyCache
{
    internal class GenericProxyCache<T>
    {
        ObjectCache cache;

        public GenericProxyCache(string id)
        {
            cache = new MemoryCache(id);
        }

        public T Get(string CacheItemName)
        {
            T CacheItem = (T)cache.Get(CacheItemName);

            if (CacheItem == null)
            {
                var newItem = Activator.CreateInstance(typeof(T), CacheItemName);
                cache.Add(CacheItemName, newItem, ObjectCache.InfiniteAbsoluteExpiration);
                return (T)newItem;
            }

            return CacheItem;
        }

        public T Get(string CacheItemName, double dt_seconds)
        {
            T CacheItem = (T)cache.Get(CacheItemName);

            if (CacheItem == null)
            {
                var newItem = Activator.CreateInstance(typeof(T), CacheItemName);
                cache.Add(CacheItemName, newItem, DateTimeOffset.Now.AddSeconds(dt_seconds));
                return (T)newItem;
            }

            return CacheItem;
        }

        public T Get(string CacheItemName, DateTimeOffset dt)
        {
            T CacheItem = (T)cache.Get(CacheItemName);

            if (CacheItem == null)
            {
                var newItem = Activator.CreateInstance(typeof(T), CacheItemName);
                cache.Add(CacheItemName, newItem, dt);
                return (T)newItem;
            }

            return CacheItem;
        }
    }
}
