namespace Trading.Strategies;

public interface ITradingStrategy
{
	Task<TradingSolution> CalculateNextStep(
		string strategyConfigurationJson,
		TradingContext context, CancellationToken cancellationToken = default);
}

public class TradingSolution
{
	public required string Asset { get; set; }
	public decimal Quantity { get; set; }
	public SolutionType Type { get; set; }
}

public enum SolutionType
{
	Hold = 0,
	Buy = 1,
	Sell = 2,
}