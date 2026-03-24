namespace Api.DTO;

public record EquityPointDto(DateOnly Date, decimal Equity, decimal DailyReturn);

public record GetBotStatisticsResponse(
	string BotId,
	string Symbol,
	decimal CurrentEquity,
	decimal? Sharpe,
	decimal? Sortino,
	decimal? Calmar,
	decimal MaxDrawdownPercent,
	decimal Volatility,
	decimal WinRate,
	decimal? ProfitFactor,
	int TradeCount,
	int TotalRoundTrips,
	decimal TotalRealizedPnl,
	decimal TotalFees,
	List<EquityPointDto> EquityCurve);

public record BotStatisticsSummaryDto(
	string BotId,
	string Symbol,
	decimal CurrentEquity,
	decimal RealizedPnl,
	decimal WinRate,
	int TradeCount);

public record GetAccountStatisticsResponse(
	decimal TotalCurrentEquity,
	decimal TotalRealizedPnl,
	List<BotStatisticsSummaryDto> Bots);
