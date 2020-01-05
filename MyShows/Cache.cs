using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MyShows
{
    internal class ExpireableCache<TKey, TValue>
    {
        private static readonly TimeSpan CLEANUP_TIMER_INTERVAL = TimeSpan.FromMinutes(30);
        private readonly Dictionary<TKey, CacheItem<TValue>> _cache = new Dictionary<TKey, CacheItem<TValue>>();
        private readonly Timer _timer;

        public ExpireableCache()
        {
            _timer = new Timer(OnTimerCallback, null, CLEANUP_TIMER_INTERVAL, Timeout.InfiniteTimeSpan);
        }

        public void Store(TKey key, TValue value, TimeSpan expiresAfter)
        {
            _cache[key] = new CacheItem<TValue>(value, expiresAfter);
        }

        public TValue Get(TKey key)
        {
            if (!_cache.ContainsKey(key)) return default(TValue);

            var cached = _cache[key];
            if (DateTimeOffset.Now - cached.Created >= cached.ExpiresAfter)
            {
                _cache.Remove(key);
                return default(TValue);
            }

            return cached.Value;
        }

        private void OnTimerCallback(object state)
        {
            foreach (var item in _cache.Where(kv => DateTimeOffset.Now - kv.Value.Created >= kv.Value.ExpiresAfter).ToList())
            {
                _cache.Remove(item.Key);
            }
        }
    }

    internal class CacheItem<T>
    {
        public CacheItem(T value, TimeSpan expiresAfter)
        {
            Value = value;
            ExpiresAfter = expiresAfter;
        }

        public T Value { get; }
        internal DateTimeOffset Created { get; } = DateTimeOffset.Now;
        internal TimeSpan ExpiresAfter { get; }
    }
}
