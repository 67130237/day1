using System.Collections.Concurrent;

namespace LoanApp.MockApi.Services;

public class IdempotencyCache
{
    private readonly ConcurrentDictionary<string, (DateTime ExpireAt, string Payload)> _cache = new();
    private readonly TimeSpan _ttl = TimeSpan.FromMinutes(5);

    public bool TryGet(string key, out string? payload)
    {
        payload = null;
        if (string.IsNullOrWhiteSpace(key)) return false;
        if (_cache.TryGetValue(key, out var item))
        {
            if (item.ExpireAt > DateTime.UtcNow) { payload = item.Payload; return true; }
            _cache.TryRemove(key, out _);
        }
        return false;
    }

    public void Set(string key, string payload)
    {
        if (string.IsNullOrWhiteSpace(key)) return;
        _cache[key] = (DateTime.UtcNow.Add(_ttl), payload);
    }
}