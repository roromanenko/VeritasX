using System.Text.Json;
using Core.Domain;
using Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;

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

public class BinanceSymbolResolver : ISymbolResolver
{
	private static readonly TimeSpan _cacheExpiry = TimeSpan.FromHours(1);
	private readonly Dictionary<string, SymbolInfo?> _memoryCache;
	private readonly HttpClient _httpClient;

	public BinanceSymbolResolver(HttpClient httpClient)
	{
		_memoryCache = [];
		_httpClient = httpClient;
	}

	public async Task<SymbolInfo> ParseSymbolAsync(string symbol)
	{
		if (string.IsNullOrWhiteSpace(symbol))
			throw new ArgumentException("Symbol cannot be null or empty");

		symbol = symbol.ToUpper();

		if (_memoryCache.TryGetValue(symbol, out SymbolInfo? cached))
			return cached!;

		await RefreshSymbolCacheAsync();

		if (_memoryCache.TryGetValue(symbol, out cached))
			return cached!;

		throw new ArgumentException($"Symbol '{symbol}' not found in Binance exchange info");
	}

	private async Task RefreshSymbolCacheAsync()
	{
		try
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
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException($"Failed to fetch exchange info from Binance: {ex.Message}", ex);
		}
	}
}
