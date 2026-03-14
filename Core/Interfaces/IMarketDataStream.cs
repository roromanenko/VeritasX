using Core.Domain;

namespace Core.Interfaces;

public interface IMarketDataStream : IAsyncDisposable
{
	Task SubscribeToTicker(string symbol, Func<decimal, Task> onPrice, CancellationToken ct = default);
	Task SubscribeToKline(string symbol, TimeSpan interval, Func<Candle, Task> onCandle, CancellationToken ct = default);
	Task Unsubscribe(string symbol);
}
