using Trading.Strategies;

namespace Trading.Processors;

public interface ITradingProcessor
{
	public Task Start(ITradingStrategy strategy,
		string strategyConfigurationJson,
		TradingContext context,
		CancellationToken cancellationToken);
	public Task Stop(CancellationToken cancellationToken);
}
