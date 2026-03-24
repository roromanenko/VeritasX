namespace Core.Domain.Statistics;

public class AccountStatistics
{
	public decimal TotalEquityUsd { get; init; }
	public decimal TotalRealizedPnl { get; init; }
	public List<ExchangeSummary> ByExchange { get; init; } = [];
	public List<BotStatisticsSummary> Bots { get; init; } = [];
}

public class ExchangeSummary
{
	public required string Exchange { get; init; }
	public decimal EquityUsd { get; init; }
	public decimal AllocationPercent { get; init; }
	public int BotCount { get; init; }
	public int ActiveBotCount { get; init; }
	public List<Balance> Assets { get; init; } = [];
}
