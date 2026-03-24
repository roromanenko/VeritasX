using Infrastructure.Persistence.Entities;
using MongoDB.Bson;

namespace Infrastructure.Interfaces;

public interface IBotStatisticsRepository
{
	Task UpsertTickEquityAsync(ObjectId botId, ObjectId userId, DateOnly date,
		decimal closingEquity, decimal lastPrice, DateTimeOffset updatedAt,
		string exchange, string quoteAsset);

	Task IncrementTradeAsync(ObjectId botId, ObjectId userId, DateOnly date,
		decimal closingEquity, decimal lastPrice,
		decimal realizedPnl, decimal fee,
		bool? isWin, decimal roundTripPnl,
		string exchange, string quoteAsset);

	Task<List<BotDailyStatisticsDocument>> GetSnapshotsAsync(ObjectId botId, DateOnly? from, DateOnly? to);
	Task<List<BotDailyStatisticsDocument>> GetLatestSnapshotPerBotAsync(ObjectId userId);

	Task InsertProcessedTradeAsync(StatisticsProcessedTradeDocument doc);
	Task PushOpenPositionAsync(ObjectId botId, string symbol, OpenPositionEntry entry);
	Task<OpenPositionEntry?> PopOpenPositionAsync(ObjectId botId, string symbol);
}
