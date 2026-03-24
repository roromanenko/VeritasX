using Core.Domain;

namespace Core.Domain.Statistics;

public abstract record StatisticsEvent(string BotId, string UserId, DateTimeOffset Timestamp);

public record MarketTickEvent(
	string BotId,
	string UserId,
	DateTimeOffset Timestamp,
	string Symbol,
	string Exchange,
	string QuoteAsset,
	decimal Price,
	decimal Equity)
	: StatisticsEvent(BotId, UserId, Timestamp);

public record TradeExecutedEvent(
	string BotId,
	string UserId,
	DateTimeOffset Timestamp,
	string TradeId,
	string Symbol,
	string Exchange,
	string QuoteAsset,
	OrderSide Side,
	decimal Price,
	decimal Quantity,
	decimal QuoteQuantity,
	decimal Fee,
	string? FeeAsset,
	decimal EquityAfterTrade)
	: StatisticsEvent(BotId, UserId, Timestamp);
