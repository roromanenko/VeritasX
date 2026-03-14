using Core.Domain;

namespace Trading.Strategies;

public interface ITradingStrategy
{
	Task<TradingSolution> CalculateNextStep(TradingContext context, CancellationToken cancellationToken = default);
}

