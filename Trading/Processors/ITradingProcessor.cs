using Trading.Strategies;

namespace Trading.Processors;

public interface ITradingProcessor
{
	public Task Start(CancellationToken cancellationToken);
	public Task Stop(CancellationToken cancellationToken);
}
