using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace TransferPlatform.Api.Helpers;

public interface ICacheWrapper
{
    bool TryGetValue<TItem>(string key, out TItem value);
    void Set<TItem>(string key, TItem value, DateTimeOffset absoluteExpiration);
}

public class MemoryCacheWrapper : ICacheWrapper
{
    private readonly IMemoryCache _memoryCache;

    public MemoryCacheWrapper(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public bool TryGetValue<TItem>(string key, out TItem value)
    {
        return _memoryCache.TryGetValue(key, out value);
    }

    public void Set<TItem>(string key, TItem value, DateTimeOffset absoluteExpiration)
    {
        _memoryCache.Set(key, value, absoluteExpiration);
    }
}

public class DoNothingMemoryCache : ICacheWrapper
{
    private readonly IMemoryCache _memoryCache;

    public DoNothingMemoryCache(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public bool TryGetValue<TItem>(string key, out TItem value)
    {
        value = default;
        return false;
    }

    public void Set<TItem>(string key, TItem value, DateTimeOffset absoluteExpiration)
    {
        // do nothing
    }
}

public class RedisCacheWrapper : ICacheWrapper
{
    private readonly IDistributedCache _memoryCache;

    public RedisCacheWrapper(IDistributedCache distributedCache)
    {
        _memoryCache = distributedCache;
    }

    public bool TryGetValue<TItem>(string key, out TItem value)
    {
        var cachedData = _memoryCache.GetString(key);
        if (!string.IsNullOrEmpty(cachedData))
        {
            value = JsonSerializer.Deserialize<TItem>(cachedData);
            return true;
        }

        value = default;
        return false;
    }

    public void Set<TItem>(string key, TItem value, DateTimeOffset absoluteExpiration)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = absoluteExpiration
        };

        var serializedData = JsonSerializer.Serialize(value);
        _memoryCache.SetString(key, serializedData, options);
    }
}