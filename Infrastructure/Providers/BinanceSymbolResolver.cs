using System.Collections.Concurrent;
using System.Text.Json;
using Core.Domain;
using Core.Interfaces;

namespace Infrastructure.Providers;

file class BinanceExchangeInfo
{
	public List<BinanceSymbolDto> Symbols { get; set; } = [];
}

file class BinanceSymbolDto
{
	public string Symbol { get; set; } = "";
	public string BaseAsset { get; set; } = "";
	public string QuoteAsset { get; set; } = "";
}

/// <summary>
/// Resolves Binance trading symbols by fetching and caching exchange metadata from the Binance REST API.<br/>
/// On the first request or after cache expiry, fetches the full symbol list from <c>/api/v3/exchangeInfo</c>
/// and populates a static in-memory cache shared across all instances.<br/>
/// Subsequent lookups within the cache window are served locally without additional HTTP calls.
/// </summary>
public class BinanceSymbolResolver : ISymbolResolver
{
	private static readonly TimeSpan _cacheExpiry = TimeSpan.FromHours(1);
	private static readonly ConcurrentDictionary<string, SymbolInfo> _memoryCache = new();
	private static DateTime _lastRefreshed = DateTime.MinValue;

	private readonly HttpClient _httpClient;

	public BinanceSymbolResolver(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	/// <summary>
	/// Resolves a Binance trading symbol to its metadata, including base and quote assets.<br/>
	/// Serves the result from the in-memory cache if it is still valid; otherwise refreshes the cache
	/// by fetching the full symbol list from the Binance API before resolving.
	/// </summary>
	/// <param name="symbol">The trading symbol to resolve, e.g. <c>BTCUSDT</c>. Case-insensitive.</param>
	/// <returns>A <see cref="SymbolInfo"/> containing the symbol, base asset, and quote asset.</returns>
	/// <exception cref="ArgumentException">
	/// Thrown if <paramref name="symbol"/> is null or whitespace,
	/// or if it is not found in the Binance exchange info.
	/// </exception>
	public async Task<SymbolInfo> ParseSymbolAsync(string symbol)
	{
		if (string.IsNullOrWhiteSpace(symbol))
			throw new ArgumentException("Symbol cannot be null or empty");

		symbol = symbol.ToUpper();

		if (DateTime.UtcNow - _lastRefreshed < _cacheExpiry
			&& _memoryCache.TryGetValue(symbol, out var cached))
			return cached;

		await RefreshSymbolCacheAsync();

		if (_memoryCache.TryGetValue(symbol, out var result))
			return result;

		throw new ArgumentException($"Symbol '{symbol}' not found in Binance exchange info");
	}

	/// <summary>
	/// Fetches the full list of trading symbols from <c>/api/v3/exchangeInfo</c>
	/// and repopulates the static in-memory cache. Updates the cache timestamp on success.
	/// </summary>
	/// <exception cref="InvalidOperationException">
	/// Thrown if the response cannot be deserialized into a valid exchange info object.
	/// </exception>
	/// <exception cref="HttpRequestException">
	/// Thrown if the HTTP request to the Binance API fails.
	/// </exception>
	private async Task RefreshSymbolCacheAsync()
	{
		const string url = "https://api.binance.com/api/v3/exchangeInfo";
		var response = await _httpClient.GetStringAsync(url);

		var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
		var exchangeInfo = JsonSerializer.Deserialize<BinanceExchangeInfo>(response, options);

		if (exchangeInfo?.Symbols == null)
			throw new InvalidOperationException("Failed to parse exchange info from Binance");

		foreach (var s in exchangeInfo.Symbols)
		{
			_memoryCache[s.Symbol] = new SymbolInfo
			{
				Symbol = s.Symbol,
				BaseAsset = s.BaseAsset,
				QuoteAsset = s.QuoteAsset
			};
		}

		_lastRefreshed = DateTime.UtcNow;
	}
}
