namespace Core.Domain.Statistics;

public class BotStatisticsSummary
{
	public string BotId { get; init; } = string.Empty;
	public string Symbol { get; init; } = string.Empty;
	public string Exchange { get; init; } = string.Empty;
	public string QuoteAsset { get; init; } = string.Empty;
	public decimal CurrentEquity { get; init; }
	public decimal CurrentEquityUsd { get; init; }
	public decimal RealizedPnl { get; init; }
	public decimal WinRate { get; init; }
	public int TradeCount { get; init; }
}
