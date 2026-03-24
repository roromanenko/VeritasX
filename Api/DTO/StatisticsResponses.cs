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

public record ExchangeSummaryDto(
	string Exchange,
	decimal EquityUsd,
	decimal AllocationPercent,
	int BotCount,
	int ActiveBotCount);

public record BotStatisticsSummaryDto(
	string BotId,
	string Symbol,
	string Exchange,
	string QuoteAsset,
	decimal CurrentEquity,
	decimal CurrentEquityUsd,
	decimal RealizedPnl,
	decimal WinRate,
	int TradeCount);

public record GetAccountStatisticsResponse(
	decimal TotalEquityUsd,
	decimal TotalRealizedPnl,
	List<ExchangeSummaryDto> ByExchange,
	List<BotStatisticsSummaryDto> Bots);

public record AssetPositionDto(
	string Asset,
	decimal Free,
	decimal Locked,
	decimal Total,
	decimal UsdValue);

public record ExchangePortfolioDto(
	string Exchange,
	decimal EquityUsd,
	decimal AllocationPercent,
	List<AssetPositionDto> Assets);

public record GetPortfolioSnapshotResponse(
	decimal TotalEquityUsd,
	List<ExchangePortfolioDto> ByExchange);
