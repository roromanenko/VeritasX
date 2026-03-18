using Core.Domain;

namespace Trading.Strategies;

public interface IStrategyFactory
{
	ITradingStrategy Create(StrategyDefinition definition);
}
