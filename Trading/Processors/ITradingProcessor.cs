using Trading.Strategies;

namespace Trading.Processors;

public interface ITradingProcessor
{
	public Task<TradingResult> Start(CancellationToken cancellationToken);
	public Task Stop(CancellationToken cancellationToken);
}
