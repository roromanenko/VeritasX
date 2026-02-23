namespace Api.DTO;

public class TradeOnHistoryDataRequest
{
	public decimal TargetWeight { get; set; } = .5m;
	public decimal Threshold { get; set; } = .1m;
	public decimal? MinQty { get; set; }
	public decimal? MinNotional { get; set; }

	public required string JobId { get; set; }
	public decimal InitBaselineQuantity { get; set; }
}

public class TradingResultDto
{
	public decimal StartTotalInBaseline { get; set; }
	public decimal EndTotalInBaseline { get; set; }
	public decimal ProfitInBaseline { get; set; }
	public decimal JustHoldTotalInBaseline { get; set; }

	/// <summary>
	/// Annualized Sharpe ratio of the strategy, calculated from per-candle portfolio returns.<br/>
	/// Measures risk-adjusted performance — how much return is generated per unit of volatility.<br/>
	/// Higher values indicate better return relative to risk.
	/// </summary>
	public decimal SharpeStrategy { get; set; }

	/// <summary>
	/// Annualized Sharpe ratio of the passive buy-and-hold benchmark, calculated from per-candle portfolio returns.<br/>
	/// Used as a baseline to evaluate whether the rebalancing strategy delivers superior risk-adjusted performance.
	/// </summary>
	public decimal SharpeHold { get; set; }
}
