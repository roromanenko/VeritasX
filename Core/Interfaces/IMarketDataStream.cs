using Core.Domain;

namespace Core.Interfaces;

public interface IMarketDataStream : IAsyncDisposable
{
	Task SubscribeToTickerAsync(string symbol, Action<decimal> onPrice, CancellationToken ct = default);
	Task SubscribeToKlineAsync(string symbol, TimeSpan interval, Action<Candle> onCandle, CancellationToken ct = default);
	Task UnsubscribeAsync(string symbol);
}
