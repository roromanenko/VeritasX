using Core.Domain;

namespace Trading.Strategies;

public interface ITradingStrategy
{
	DataRequirement DataRequirement { get; }
	Task<TradingSolution> CalculateNextStep(TradingContext context, MarketTick tick, CancellationToken ct = default);
}

public enum DataRequirement
{
	Ticker,
	Kline
}
