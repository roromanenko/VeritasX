namespace Core.Domain.Statistics;

public class AccountStatistics
{
	public decimal TotalCurrentEquity { get; init; }
	public decimal TotalRealizedPnl { get; init; }
	public List<BotStatisticsSummary> Bots { get; init; } = new();
}
