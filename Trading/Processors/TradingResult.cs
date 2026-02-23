namespace Trading.Processors;

public class TradingResult
{
	public decimal StartTotalInBaseline { get; internal set; }
	public decimal EndTotalInBaseline { get; internal set; }
	public decimal ProfitInBaseline { get; internal set; }
	public decimal JustHoldTotalInBaseline { get; internal set; }

	/// <summary>
	/// A chronological series of total portfolio values (in baseline asset) captured after each candle
	/// when the rebalancing strategy is applied. <br/>
	/// Used to calculate risk-adjusted performance metrics
	/// such as the Sharpe ratio.
	/// </summary>
	public List<decimal> PortfolioSnapshots { get; set; } = new();

	/// <summary>
	/// A chronological series of total portfolio values (in baseline asset) captured after each candle
	/// under a passive buy-and-hold scenario (initial allocation held without any trades). <br/>
	/// Used as a benchmark to compare against the active strategy.
	/// </summary>
	public List<decimal> HoldSnapshots { get; set; } = new();

	/// <summary>
	/// Annualized Sharpe ratio of the strategy, calculated from per-candle portfolio returns.<br/>
	/// Measures risk-adjusted performance — how much return is generated per unit of volatility.<br/>
	/// Higher values indicate better return relative to risk.
	/// </summary>
	public decimal SharpeStrategy { get; internal set; }

	/// <summary>
	/// Annualized Sharpe ratio of the passive buy-and-hold benchmark, calculated from per-candle portfolio returns.<br/>
	/// Used as a baseline to evaluate whether the rebalancing strategy delivers superior risk-adjusted performance.
	/// </summary>
	public decimal SharpeHold { get; internal set; }
}
