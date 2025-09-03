using Core.Domain;

namespace Core.Interfaces;

public interface IPriceProvider
{
	Task<IEnumerable<Candle>> GetHistoryAsync
	(
		string symbol,
		DateTimeOffset fromUtc,
		DateTimeOffset toUtc,
		TimeSpan interval,
		CancellationToken ct = default
	);

	Task<decimal> GetPriceAsync(string asset, string baseline, CancellationToken ct = default);
}