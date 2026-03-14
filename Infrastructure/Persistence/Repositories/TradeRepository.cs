using Core.Interfaces;
using Core.Options;
using Infrastructure.Interfaces;
using Infrastructure.Persistence.Entities;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Persistence.Repositories;

public class TradeRepository : ITradeRepository
{
	private readonly MongoDbOptions _mongoDbOptions;
	private readonly IMongoDbContext _dbContext;

	public TradeRepository(IOptions<MongoDbOptions> mongoDbOptions, IMongoDbContext dbContext)
	{
		_mongoDbOptions = mongoDbOptions.Value;
		_dbContext = dbContext;
	}

	public async Task<TradeDocument> CreateTrade(TradeDocument newTrade)
	{
		await _dbContext
			.GetCollection<TradeDocument>()
			.InsertOneAsync(newTrade);

		return newTrade;
	}

	public async Task<IEnumerable<TradeDocument>> GetTradeByDateRange(ObjectId userId, DateTimeOffset from, DateTimeOffset to)
	{
		var filter = Builders<TradeDocument>.Filter.And(
			Builders<TradeDocument>.Filter.Eq(t => t.UserId, userId),
			Builders<TradeDocument>.Filter.Gte(t => t.ExecutedAt, from),
			Builders<TradeDocument>.Filter.Lte(t => t.ExecutedAt, to)
		);

		var trades = await _dbContext
			.GetCollection<TradeDocument>()
			.Find(filter)
			.SortByDescending(t => t.ExecutedAt)
			.ToListAsync();

		return trades;
	}

	public async Task<IEnumerable<TradeDocument>> GetTradeByExchangeOrderId(ObjectId userId, string exchangeOrderId)
	{
		var filter = Builders<TradeDocument>.Filter.And(
			Builders<TradeDocument>.Filter.Eq(t => t.UserId, userId),
			Builders<TradeDocument>.Filter.Eq(t => t.ExchangeOrderId, exchangeOrderId)
		);

		var trades = await _dbContext
			.GetCollection<TradeDocument>()
			.Find(filter)
			.SortByDescending(t => t.ExecutedAt)
			.ToListAsync();

		return trades;
	}

	public async Task<TradeDocument?> GetTradeById(ObjectId tradeId)
	{
		var filter = Builders<TradeDocument>.Filter.Eq(t => t.Id, tradeId);

		var trade = await _dbContext
			.GetCollection<TradeDocument>()
			.Find(filter)
			.FirstOrDefaultAsync();

		return trade;
	}

	public async Task<IEnumerable<TradeDocument>> GetTradeBySymbol(ObjectId userId, string symbol, int limit = 100)
	{
		var filter = Builders<TradeDocument>.Filter.And(
			Builders<TradeDocument>.Filter.Eq(t => t.UserId, userId),
			Builders<TradeDocument>.Filter.Eq(t => t.Symbol, symbol)
		);

		var trades = await _dbContext
			.GetCollection<TradeDocument>()
			.Find(filter)
			.SortByDescending(t => t.ExecutedAt)
			.Limit(limit)
			.ToListAsync();

		return trades;
	}

	public async Task<IEnumerable<TradeDocument>> GetTradeByUserId(ObjectId userId, int limit = 100)
	{
		var filter = Builders<TradeDocument>.Filter.Eq(t => t.UserId, userId);

		var trades = await _dbContext
			.GetCollection<TradeDocument>()
			.Find(filter)
			.SortByDescending(t => t.ExecutedAt)
			.Limit(limit)
			.ToListAsync();

		return trades;
	}
}
