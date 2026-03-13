using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Core.Domain;
using Core.Interfaces;

namespace Infrastructure.Providers;

public class BinancePriceProvider : IPriceProvider
{
	private readonly HttpClient _httpClient;
	private readonly ISymbolResolver _symbolResolver;

	public BinancePriceProvider(HttpClient httpClient, ISymbolResolver symbolResolver)
	{
		_httpClient = httpClient;
		_symbolResolver = symbolResolver;
	}

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
