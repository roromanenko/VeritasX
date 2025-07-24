using VeritasX.Core.Domain;

namespace VeritasX.Core.Interfaces;

public interface ICachedPriceProvider
{
	Task<IEnumerable<Candle>> GetHistoryAsync
	(
		string symbol,
		DateTime fromUtc,
		DateTime toUtc,
		TimeSpan interval,
		CancellationToken ct = default
	);
}