using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Interfaces;
using Core.Options;
using Infrastructure.Interfaces;
using Infrastructure.Persistence.Entities;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Persistence.Repositories;

public class BotTradeRepository : IBotTradeRepository
{
	private readonly MongoDbOptions _mongoDbOptions;
	private readonly IMongoDbContext _dbContext;

	public BotTradeRepository(IOptions<MongoDbOptions> mongoDbOptions, IMongoDbContext dbContext)
	{
		_mongoDbOptions = mongoDbOptions.Value;
		_dbContext = dbContext;
	}

	public async Task<BotTradeRecordDocument> CreateTradeRecord(BotTradeRecordDocument record)
	{
		await _dbContext
			.GetCollection<BotTradeRecordDocument>()
			.InsertOneAsync(record);
		return record;
	}

	public async Task<IEnumerable<BotTradeRecordDocument>> GetTradesByBotId(ObjectId botId, int limit = 100)
	{
		var filter = Builders<BotTradeRecordDocument>.Filter.Eq(r => r.BotId, botId);
		return await _dbContext
			.GetCollection<BotTradeRecordDocument>()
			.Find(filter)
			.SortByDescending(r => r.ExecutedAt)
			.Limit(limit)
			.ToListAsync();
	}

	public async Task<IEnumerable<BotTradeRecordDocument>> GetTradesByUserId(ObjectId userId, int limit = 100)
	{
		var filter = Builders<BotTradeRecordDocument>.Filter.Eq(r => r.UserId, userId);
		return await _dbContext
			.GetCollection<BotTradeRecordDocument>()
			.Find(filter)
			.SortByDescending(r => r.ExecutedAt)
			.Limit(limit)
			.ToListAsync();
	}

	public async Task<IEnumerable<BotTradeRecordDocument>> GetTradesByDateRange(ObjectId botId, DateTimeOffset from, DateTimeOffset to)
	{
		var filter = Builders<BotTradeRecordDocument>.Filter.And(
			Builders<BotTradeRecordDocument>.Filter.Eq(r => r.BotId, botId),
			Builders<BotTradeRecordDocument>.Filter.Gte(r => r.ExecutedAt, from),
			Builders<BotTradeRecordDocument>.Filter.Lte(r => r.ExecutedAt, to)
		);
		return await _dbContext
			.GetCollection<BotTradeRecordDocument>()
			.Find(filter)
			.SortByDescending(r => r.ExecutedAt)
			.ToListAsync();
	}
}
