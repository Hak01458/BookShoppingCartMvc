using System;
using System.Collections.Concurrent;

namespace BookShoppingCartMvcUI.Shared
{
    public interface IAppCache
    {
        T? Get<T>(string key);
        void Set<T>(string key, T value, TimeSpan? ttl = null);
        bool TryGetValue<T>(string key, out T? value);
        void Remove(string key);
        void Clear();
    }

    internal class CacheItem
    {
        public object Value { get; }
        public DateTime? Expiration { get; }

        public CacheItem(object value, DateTime? expiration)
        {
            Value = value;
            Expiration = expiration;
        }

        public bool IsExpired => Expiration.HasValue && DateTime.UtcNow > Expiration.Value;
    }

    public class AppCache : IAppCache
    {
        private readonly ConcurrentDictionary<string, CacheItem> _cache = new();

        public T? Get<T>(string key)
        {
            if (TryGetValue(key, out T? value))
                return value;
            return default;
        }

        public bool TryGetValue<T>(string key, out T? value)
        {
            value = default;
            if (_cache.TryGetValue(key, out var item))
            {
                if (item.IsExpired)
                {
                    _cache.TryRemove(key, out _);
                    return false;
                }

                if (item.Value is T t)
                {
                    value = t;
                    return true;
                }

                // attempt to convert/cast if possible
                try
                {
                    value = (T)item.Value;
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        public void Set<T>(string key, T value, TimeSpan? ttl = null)
        {
            DateTime? expiration = ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : null;
            var item = new CacheItem(value!, expiration);
            _cache.AddOrUpdate(key, item, (_, __) => item);
        }

        public void Remove(string key)
        {
            _cache.TryRemove(key, out _);
        }

        public void Clear()
        {
            _cache.Clear();
        }
    }
}
