using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Core.Domain;
using Core.Interfaces;

namespace Infrastructure.Providers;

/// <summary>
/// Provides market data for Binance trading pairs by querying the Binance REST API.
/// Resolves asset pairs to valid Binance symbols via <see cref="ISymbolResolver"/>
/// before making any data requests.
/// </summary>
public class BinancePriceProvider : IPriceProvider
{
	private readonly HttpClient _httpClient;
	private readonly ISymbolResolver _symbolResolver;

	public BinancePriceProvider(HttpClient httpClient, ISymbolResolver symbolResolver)
	{
		_httpClient = httpClient;
		_symbolResolver = symbolResolver;
	}

	/// <summary>
	/// Fetches historical candlestick data for the given asset pair within the specified time range.
	/// </summary>
	/// <param name="asset">The base asset, e.g. <c>BTC</c>.</param>
	/// <param name="baseline">The quote asset, e.g. <c>USDT</c>.</param>
	/// <param name="fromUtc">The start of the time range (inclusive).</param>
	/// <param name="toUtc">The end of the time range (inclusive).</param>
	/// <param name="interval">The candlestick interval, e.g. 1 minute, 1 hour.</param>
	/// <param name="ct">Cancellation token.</param>
	/// <returns>A collection of <see cref="Candle"/> ordered by open time, or an empty collection if no data is returned.</returns>
	/// <exception cref="ArgumentException">Thrown if the combined symbol is not found in Binance exchange info.</exception>
	/// <exception cref="HttpRequestException">Thrown if the request to the Binance API fails.</exception>
	public async Task<IEnumerable<Candle>> GetHistoryAsync(
		string asset,
		string baseline,
		DateTimeOffset fromUtc,
		DateTimeOffset toUtc,
		TimeSpan interval,
		CancellationToken ct = default)
	{
		var symbolInfo = await _symbolResolver.ParseSymbolAsync($"{asset}{baseline}");

		long startMs = fromUtc.ToUnixTimeMilliseconds();
		long endMs = toUtc.ToUnixTimeMilliseconds();
		string intervalStr = ToIntervalString(interval);

		string url = $"https://api.binance.com/api/v3/klines?symbol={symbolInfo.Symbol}&interval={intervalStr}&startTime={startMs}&endTime={endMs}";

		using var resp = await _httpClient.GetAsync(url, ct);
		resp.EnsureSuccessStatusCode();

		using var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>(ct);
		if (doc == null) return [];

		var list = new List<Candle>();
		foreach (var item in doc.RootElement.EnumerateArray())
		{
			long openTime = item[0].GetInt64();
			decimal open = decimal.Parse(item[1].GetString() ?? "0", CultureInfo.InvariantCulture);
			decimal high = decimal.Parse(item[2].GetString() ?? "0", CultureInfo.InvariantCulture);
			decimal low = decimal.Parse(item[3].GetString() ?? "0", CultureInfo.InvariantCulture);
			decimal close = decimal.Parse(item[4].GetString() ?? "0", CultureInfo.InvariantCulture);
			decimal volume = decimal.Parse(item[5].GetString() ?? "0", CultureInfo.InvariantCulture);
			DateTime openTimeUtc = DateTimeOffset.FromUnixTimeMilliseconds(openTime).UtcDateTime;
			list.Add(new Candle(openTimeUtc, open, high, low, close, volume));
		}

		return list;
	}

	/// <summary>
	/// Fetches the current market price for the given asset pair.
	/// </summary>
	/// <param name="asset">The base asset, e.g. <c>BTC</c>.</param>
	/// <param name="baseline">The quote asset, e.g. <c>USDT</c>.</param>
	/// <param name="ct">Cancellation token.</param>
	/// <returns>The latest price as a <see cref="decimal"/>, or <c>0</c> if the response is empty.</returns>
	/// <exception cref="ArgumentException">Thrown if the combined symbol is not found in Binance exchange info.</exception>
	/// <exception cref="HttpRequestException">Thrown if the request to the Binance API fails.</exception>
	public async Task<decimal> GetPriceAsync(string asset, string baseline, CancellationToken ct = default)
	{
		var symbolInfo = await _symbolResolver.ParseSymbolAsync($"{asset}{baseline}");

		string url = $"https://api.binance.com/api/v3/ticker/price?symbol={symbolInfo.Symbol}";
		using var resp = await _httpClient.GetAsync(url, ct);
		resp.EnsureSuccessStatusCode();

		using var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>(ct);
		if (doc == null) return 0m;

		return decimal.Parse(
			doc.RootElement.GetProperty("price").GetString() ?? "0",
			CultureInfo.InvariantCulture
		);
	}

	private static string ToIntervalString(TimeSpan interval)
	{
		if (interval.TotalSeconds < 60) return $"{(int)interval.TotalSeconds}s";
		if (interval.TotalMinutes < 60) return $"{(int)interval.TotalMinutes}m";
		if (interval.TotalHours < 24) return $"{(int)interval.TotalHours}h";
		return $"{(int)interval.TotalDays}d";
	}
}
