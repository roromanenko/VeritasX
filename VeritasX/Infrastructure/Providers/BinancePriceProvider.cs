using System.Net.Http;
using System.Text.Json;
using System.Globalization;
using VeritasX.Core.Domain;
using VeritasX.Core.Interfaces;

namespace VeritasX.Infrastructure.Providers;

public class BinancePriceProvider : IPriceProvider 
{
	private readonly HttpClient _httpClient;

	public BinancePriceProvider(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	public async Task<IEnumerable<Candle>> GetHistoryAsync(string symbol, DateTime fromUtc, DateTime toUtc, TimeSpan interval, CancellationToken ct = default)
	{
		Console.WriteLine("Binance cache test");

		long startMs = ((DateTimeOffset)fromUtc).ToUnixTimeMilliseconds();
		long endMs   = ((DateTimeOffset)toUtc).ToUnixTimeMilliseconds();

		string unit;
		int value;

		if (interval.TotalSeconds < 60)
		{
			unit = "s";
			value = (int)interval.TotalSeconds;
		}
		else if (interval.TotalMinutes < 60)
		{
			unit = "m";
			value = (int)interval.TotalMinutes;
		}
		else if (interval.TotalHours < 24)
		{
			unit = "h";
			value = (int)interval.TotalHours;
		}
		else
		{
			unit = "d";
			value = (int)interval.TotalDays;
		}

		string intervalStr = $"{value}{unit}";

		string url = $"https://api.binance.com/api/v3/klines?symbol={symbol}&interval={intervalStr}&startTime={startMs}&endTime={endMs}";
		
		using var resp = await _httpClient.GetAsync(url, ct);
		resp.EnsureSuccessStatusCode();
		using var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>(ct);

		if (doc == null)
		{
			return [];
		}

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
}