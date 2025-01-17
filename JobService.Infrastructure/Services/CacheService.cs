namespace JobService.Infrastructure.Services;
[AutoInterface]

public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;

    public CacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }
    
    public async Task<T?> GetData<T>(string key)
    {
        if (_memoryCache.TryGetValue(key, out T? value))
            return await Task.FromResult(value);
        else return default; 
    }

    public async Task SetData<T>(string key, T value, DateTime expirationTime)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromSeconds(180))
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
            .SetPriority(CacheItemPriority.Normal);

        _memoryCache.Set(key, value, cacheEntryOptions);
        await Task.CompletedTask;
    }

    public async Task RemoveData(string key)
    {
        _memoryCache.Remove(key);
        await Task.CompletedTask;
    }
}