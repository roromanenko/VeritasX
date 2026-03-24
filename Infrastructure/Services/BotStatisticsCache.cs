using Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Services;

public class BotStatisticsCache : IStatisticsCache
{
	private readonly IMemoryCache _cache;

	public BotStatisticsCache(IMemoryCache cache)
	{
		_cache = cache;
	}

	public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
	{
		_cache.TryGetValue(key, out T? value);
		return Task.FromResult(value);
	}

	public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default)
	{
		var options = new MemoryCacheEntryOptions
		{
			AbsoluteExpirationRelativeToNow = ttl,
			Size = 1
		};
		_cache.Set(key, value, options);
		return Task.CompletedTask;
	}

	public Task InvalidateAsync(string key, CancellationToken ct = default)
	{
		_cache.Remove(key);
		return Task.CompletedTask;
	}
}
