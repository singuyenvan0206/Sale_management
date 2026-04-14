using System.Collections.Concurrent;

namespace FashionStore.Services
{
    public class InMemoryCacheService : ICacheService
    {
        private class CacheEntry
        {
            public object Value { get; set; } = null!;
            public DateTime? Expiration { get; set; }
        }

        private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();

        public T? Get<T>(string key)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                if (entry.Expiration.HasValue && entry.Expiration.Value < DateTime.Now)
                {
                    _cache.TryRemove(key, out _);
                    return default;
                }
                return (T)entry.Value;
            }
            return default;
        }

        public void Set<T>(string key, T value, TimeSpan? expiration = null)
        {
            if (value == null) return;

            var entry = new CacheEntry
            {
                Value = value,
                Expiration = expiration.HasValue ? DateTime.Now.Add(expiration.Value) : null
            };

            _cache.AddOrUpdate(key, entry, (_, _) => entry);
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
