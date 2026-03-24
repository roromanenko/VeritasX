using Core.Interfaces;
using Infrastructure.Interfaces;
using Infrastructure.Persistence.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Persistence.Repositories;

public class BotStatisticsRepository : IBotStatisticsRepository
{
	private readonly IMongoDbContext _dbContext;

	public BotStatisticsRepository(IMongoDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task UpsertTickEquityAsync(ObjectId botId, ObjectId userId, DateOnly date,
		decimal closingEquity, decimal lastPrice, DateTimeOffset updatedAt,
		string exchange, string quoteAsset)
	{
		var collection = _dbContext.GetCollection<BotDailyStatisticsDocument>();
		var filter = Builders<BotDailyStatisticsDocument>.Filter.And(
			Builders<BotDailyStatisticsDocument>.Filter.Eq(d => d.BotId, botId),
			Builders<BotDailyStatisticsDocument>.Filter.Eq(d => d.Date, date));

		var update = Builders<BotDailyStatisticsDocument>.Update
			.Set(d => d.ClosingEquity, closingEquity)
			.Set(d => d.LastPrice, lastPrice)
			.Set(d => d.LastUpdatedAt, updatedAt)
			.SetOnInsert(d => d.BotId, botId)
			.SetOnInsert(d => d.UserId, userId)
			.SetOnInsert(d => d.Date, date)
			.SetOnInsert(d => d.OpeningEquity, closingEquity)
			.SetOnInsert(d => d.Exchange, exchange)
			.SetOnInsert(d => d.QuoteAsset, quoteAsset);

		await collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
	}

	public async Task IncrementTradeAsync(ObjectId botId, ObjectId userId, DateOnly date,
		decimal closingEquity, decimal lastPrice,
		decimal realizedPnl, decimal fee,
		bool? isWin, decimal roundTripPnl,
		string exchange, string quoteAsset)
	{
		var collection = _dbContext.GetCollection<BotDailyStatisticsDocument>();
		var filter = Builders<BotDailyStatisticsDocument>.Filter.And(
			Builders<BotDailyStatisticsDocument>.Filter.Eq(d => d.BotId, botId),
			Builders<BotDailyStatisticsDocument>.Filter.Eq(d => d.Date, date));

		var update = Builders<BotDailyStatisticsDocument>.Update
			.Set(d => d.ClosingEquity, closingEquity)
			.Set(d => d.LastPrice, lastPrice)
			.Set(d => d.LastUpdatedAt, DateTimeOffset.UtcNow)
			.SetOnInsert(d => d.BotId, botId)
			.SetOnInsert(d => d.UserId, userId)
			.SetOnInsert(d => d.Date, date)
			.SetOnInsert(d => d.OpeningEquity, closingEquity)
			.SetOnInsert(d => d.Exchange, exchange)
			.SetOnInsert(d => d.QuoteAsset, quoteAsset)
			.Inc(d => d.RealizedPnl, realizedPnl)
			.Inc(d => d.Fees, fee)
			.Inc(d => d.TradeCount, 1);

		if (isWin.HasValue)
		{
			var absPnl = Math.Abs(roundTripPnl);
			if (isWin.Value)
			{
				update = update
					.Inc(d => d.WinCount, 1)
					.Inc(d => d.GrossProfit, absPnl)
					.Max(d => d.LargestWin, absPnl)
					.Inc(d => d.CurrentConsecutiveWins, 1)
					.Set(d => d.CurrentConsecutiveLosses, 0);
			}
			else
			{
				update = update
					.Inc(d => d.LossCount, 1)
					.Inc(d => d.GrossLoss, absPnl)
					.Max(d => d.LargestLoss, absPnl)
					.Inc(d => d.CurrentConsecutiveLosses, 1)
					.Set(d => d.CurrentConsecutiveWins, 0);
			}
		}

		await collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });

		// Update max consecutive streaks in a second pass (requires reading current values)
		if (isWin.HasValue)
		{
			var doc = await collection.Find(filter).FirstOrDefaultAsync();
			if (doc is not null)
			{
				var streakUpdate = isWin.Value
					? Builders<BotDailyStatisticsDocument>.Update.Max(d => d.MaxConsecutiveWins, doc.CurrentConsecutiveWins)
					: Builders<BotDailyStatisticsDocument>.Update.Max(d => d.MaxConsecutiveLosses, doc.CurrentConsecutiveLosses);
				await collection.UpdateOneAsync(filter, streakUpdate);
			}
		}
	}

	public async Task<List<BotDailyStatisticsDocument>> GetSnapshotsAsync(ObjectId botId, DateOnly? from, DateOnly? to)
	{
		var collection = _dbContext.GetCollection<BotDailyStatisticsDocument>();
		var filters = new List<FilterDefinition<BotDailyStatisticsDocument>>
		{
			Builders<BotDailyStatisticsDocument>.Filter.Eq(d => d.BotId, botId)
		};

		if (from.HasValue)
			filters.Add(Builders<BotDailyStatisticsDocument>.Filter.Gte(d => d.Date, from.Value));
		if (to.HasValue)
			filters.Add(Builders<BotDailyStatisticsDocument>.Filter.Lte(d => d.Date, to.Value));

		return await collection
			.Find(Builders<BotDailyStatisticsDocument>.Filter.And(filters))
			.SortBy(d => d.Date)
			.ToListAsync();
	}

	public async Task<List<BotDailyStatisticsDocument>> GetLatestSnapshotPerBotAsync(ObjectId userId)
	{
		var collection = _dbContext.GetCollection<BotDailyStatisticsDocument>();

		var pipeline = new[]
		{
			new BsonDocument("$match", new BsonDocument("userId", userId)),
			new BsonDocument("$sort", new BsonDocument("date", -1)),
			new BsonDocument("$group", new BsonDocument
			{
				{ "_id", "$botId" },
				{ "doc", new BsonDocument("$first", "$$ROOT") }
			}),
			new BsonDocument("$replaceRoot", new BsonDocument("newRoot", "$doc"))
		};

		return await collection
			.Aggregate<BotDailyStatisticsDocument>(pipeline)
			.ToListAsync();
	}

	public async Task InsertProcessedTradeAsync(StatisticsProcessedTradeDocument doc)
	{
		var collection = _dbContext.GetCollection<StatisticsProcessedTradeDocument>();
		await collection.InsertOneAsync(doc);
	}

	public async Task PushOpenPositionAsync(ObjectId botId, string symbol, OpenPositionEntry entry)
	{
		var collection = _dbContext.GetCollection<BotOpenPositionsDocument>();
		var filter = Builders<BotOpenPositionsDocument>.Filter.And(
			Builders<BotOpenPositionsDocument>.Filter.Eq(d => d.BotId, botId),
			Builders<BotOpenPositionsDocument>.Filter.Eq(d => d.Symbol, symbol));

		var update = Builders<BotOpenPositionsDocument>.Update
			.Push(d => d.BuyQueue, entry)
			.SetOnInsert(d => d.BotId, botId)
			.SetOnInsert(d => d.Symbol, symbol);

		await collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
	}

	public async Task<OpenPositionEntry?> PopOpenPositionAsync(ObjectId botId, string symbol)
	{
		var collection = _dbContext.GetCollection<BotOpenPositionsDocument>();
		var filter = Builders<BotOpenPositionsDocument>.Filter.And(
			Builders<BotOpenPositionsDocument>.Filter.Eq(d => d.BotId, botId),
			Builders<BotOpenPositionsDocument>.Filter.Eq(d => d.Symbol, symbol));

		var update = Builders<BotOpenPositionsDocument>.Update.PopFirst(d => d.BuyQueue);

		var result = await collection.FindOneAndUpdateAsync(filter, update,
			new FindOneAndUpdateOptions<BotOpenPositionsDocument> { ReturnDocument = ReturnDocument.Before });

		return result?.BuyQueue.FirstOrDefault();
	}
}
