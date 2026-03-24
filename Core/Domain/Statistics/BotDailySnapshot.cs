namespace Core.Domain.Statistics;

public class BotDailySnapshot
{
	public string BotId { get; init; } = string.Empty;
	public string UserId { get; init; } = string.Empty;
	public DateOnly Date { get; init; }

	public decimal OpeningEquity { get; init; }
	public decimal ClosingEquity { get; init; }
	public decimal LastPrice { get; init; }
	public DateTimeOffset LastUpdatedAt { get; init; }

	public decimal RealizedPnl { get; init; }
	public decimal Fees { get; init; }
	public int TradeCount { get; init; }

	public int WinCount { get; init; }
	public int LossCount { get; init; }
	public decimal GrossProfit { get; init; }
	public decimal GrossLoss { get; init; }
	public decimal LargestWin { get; init; }
	public decimal LargestLoss { get; init; }
	public int MaxConsecutiveWins { get; init; }
	public int MaxConsecutiveLosses { get; init; }
}
