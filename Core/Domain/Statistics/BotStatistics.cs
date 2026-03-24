namespace Core.Domain.Statistics;

public class BotStatistics
{
	public string BotId { get; init; } = string.Empty;
	public string Symbol { get; init; } = string.Empty;
	public decimal CurrentEquity { get; init; }

	public decimal? Sharpe { get; init; }
	public decimal? Sortino { get; init; }
	public decimal? Calmar { get; init; }
	public decimal MaxDrawdownPercent { get; init; }
	public decimal Volatility { get; init; }

	public decimal WinRate { get; init; }
	public decimal? ProfitFactor { get; init; }
	public int TradeCount { get; init; }
	public int TotalRoundTrips { get; init; }

	public decimal TotalRealizedPnl { get; init; }
	public decimal TotalFees { get; init; }

	public List<EquityPoint> EquityCurve { get; init; } = new();
}
