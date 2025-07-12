using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VeritasX.Core.Interfaces;
using VeritasX.Core.Domain;
using VeritasX.Core.Options;

namespace VeritasX.Infrastructure.Providers;

public class CachedPriceProvider : ICachedPriceProvider
{
    private readonly IPriceProvider _inner;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedPriceProvider> _logger;
    private readonly CacheOptions _cacheOptions;

    public CachedPriceProvider(
        IPriceProvider inner, 
        IMemoryCache cache, 
        ILogger<CachedPriceProvider> logger,
        IOptions<CacheOptions> cacheOptions)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
        _cacheOptions = cacheOptions.Value;
    }

    public async Task<IEnumerable<Candle>> GetHistoryAsync(
        string symbol, 
        DateTime fromUtc, 
        DateTime toUtc, 
        TimeSpan interval, 
        CancellationToken ct = default)
    {
        var cacheKey = GeneratePriceHistoryKey(symbol, fromUtc, toUtc, interval);
        
        if (_cache.TryGetValue(cacheKey, out IEnumerable<Candle>? cachedData))
        {
            _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
            return cachedData ?? [];
        }

        _logger.LogDebug("Cache miss for key: {CacheKey}", cacheKey);
        
        var data = await _inner.GetHistoryAsync(symbol, fromUtc, toUtc, interval, ct);
        
        var cacheOptions = CreateCacheOptions(interval);
        _cache.Set(cacheKey, data, cacheOptions);
        
        _logger.LogDebug("Data cached with key: {CacheKey}, TTL: {TTL}", 
            cacheKey, cacheOptions.AbsoluteExpirationRelativeToNow);
        
        return data;
    }

    private MemoryCacheEntryOptions CreateCacheOptions(TimeSpan interval)
    {
        // Умное кэширование: TTL зависит от интервала данных и конфигурации
        var ttlMinutes = interval.TotalMinutes switch
        {
            <= 5 => _cacheOptions.IntervalBasedTtl.ShortTerm,
            <= 60 => _cacheOptions.IntervalBasedTtl.MediumTerm,
            <= 1440 => _cacheOptions.IntervalBasedTtl.LongTerm,
            _ => _cacheOptions.IntervalBasedTtl.VeryLongTerm
        };

        var ttl = TimeSpan.FromMinutes(ttlMinutes);

        return new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl,
            SlidingExpiration = TimeSpan.FromMinutes(ttlMinutes / 2.0),
            Priority = CacheItemPriority.Normal,
            Size = 1
        };
    }

    private string GeneratePriceHistoryKey(string symbol, DateTime fromUtc, DateTime toUtc, TimeSpan interval)
        => $"price_history:{symbol}:{fromUtc:yyyyMMddHHmm}:{toUtc:yyyyMMddHHmm}:{interval.TotalMinutes}";
}