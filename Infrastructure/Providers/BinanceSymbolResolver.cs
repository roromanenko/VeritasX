using Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace Infrastructure.Providers;

public class BinanceSymbolResolver : ISymbolResolver
{
	private static readonly TimeSpan CacheExpiry = TimeSpan.FromHours(1);
	private readonly Dictionary<string, BinanceSymbolInfo?> _memoryCache;
	private readonly HttpClient _httpClient;

	public BinanceSymbolResolver(IMemoryCache memoryCache, HttpClient httpClient)
	{
		_memoryCache = new Dictionary<string, BinanceSymbolInfo?>();
		_httpClient = httpClient;
	}

	public async Task<BinanceSymbolInfo> ParseSymbolAsync(string symbol)
	{
		if (string.IsNullOrWhiteSpace(symbol))
			throw new ArgumentException("Symbol cannot be null or empty");

		symbol = symbol.ToUpper();

		if (_memoryCache.TryGetValue(symbol, out BinanceSymbolInfo? cachedInfo))
		{
			return cachedInfo!;
		}
		if (cachedInfo == null)
		{
			await RefreshSymbolCacheAsync();
		}

		if (_memoryCache.TryGetValue(symbol, out cachedInfo))
		{
			return cachedInfo!;
		}

		throw new ArgumentException($"Symbol '{symbol}' not found in Binance exchange info");
	}

	private async Task RefreshSymbolCacheAsync()
	{
		try
		{
			const string exchangeInfoUrl = "https://api.binance.com/api/v3/exchangeInfo";
			var response = await _httpClient.GetStringAsync(exchangeInfoUrl);

			var options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			};

			var exchangeInfo = JsonSerializer.Deserialize<BinanceExchangeInfo>(response, options);
			if (exchangeInfo == null || exchangeInfo.Symbols == null)
			{
				throw new InvalidOperationException("Failed to parse exchange info from Binance");
			}

			foreach (var symbolInfo in exchangeInfo.Symbols)
			{
				var entry = new BinanceSymbolInfo
				{
					Symbol = symbolInfo.Symbol,
					BaseAsset = symbolInfo.BaseAsset,
					QuoteAsset = symbolInfo.QuoteAsset
				};
				//_memoryCache.Set(symbolInfo.Symbol!, entry, new MemoryCacheEntryOptions
				//{
				//	AbsoluteExpirationRelativeToNow = CacheExpiry,
				//	Size = entry.ObjectSizeInBytes
				//});
				_memoryCache.Add(symbolInfo.Symbol!, entry);
			}
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException($"Failed to fetch exchange info from Binance: {ex.Message}", ex);
		}
	}
}
