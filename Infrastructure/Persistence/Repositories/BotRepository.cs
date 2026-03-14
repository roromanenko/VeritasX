using Core.Domain;
using Core.Interfaces;
using Core.Options;
using Infrastructure.Interfaces;
using Infrastructure.Persistence.Entities;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Persistence.Repositories;

public class BotRepository : IBotRepository
{
	private readonly MongoDbOptions _mongoDbOptions;
	private readonly IMongoDbContext _dbContext;

	public BotRepository(IOptions<MongoDbOptions> mongoDbOptions, IMongoDbContext dbContext)
	{
		_mongoDbOptions = mongoDbOptions.Value;
		_dbContext = dbContext;
	}

	public async Task<BotConfigurationDocument> CreateBot(BotConfigurationDocument bot)
	{
		await _dbContext
			.GetCollection<BotConfigurationDocument>()
			.InsertOneAsync(bot);
		return bot;
	}

	public async Task<BotConfigurationDocument?> GetBotById(ObjectId botId, ObjectId userId)
	{
		var filter = Builders<BotConfigurationDocument>.Filter.And(
			Builders<BotConfigurationDocument>.Filter.Eq(b => b.Id, botId),
			Builders<BotConfigurationDocument>.Filter.Eq(b => b.UserId, userId)
		);
		return await _dbContext
			.GetCollection<BotConfigurationDocument>()
			.Find(filter)
			.FirstOrDefaultAsync();
	}

	public async Task<IEnumerable<BotConfigurationDocument>> GetBotsByUserId(ObjectId userId)
	{
		var filter = Builders<BotConfigurationDocument>.Filter.Eq(b => b.UserId, userId);
		return await _dbContext
			.GetCollection<BotConfigurationDocument>()
			.Find(filter)
			.ToListAsync();
	}

	public async Task<IEnumerable<BotConfigurationDocument>> GetActiveBots()
	{
		var filter = Builders<BotConfigurationDocument>.Filter.Eq(b => b.Status, BotStatus.Active);
		return await _dbContext
			.GetCollection<BotConfigurationDocument>()
			.Find(filter)
			.ToListAsync();
	}

	public Task UpdateBot(BotConfigurationDocument bot)
	{
		var filter = Builders<BotConfigurationDocument>.Filter.Eq(b => b.Id, bot.Id);
		return _dbContext
			.GetCollection<BotConfigurationDocument>()
			.ReplaceOneAsync(filter, bot);
	}

	public async Task DeleteBot(ObjectId botId, ObjectId userId)
	{
		var filter = Builders<BotConfigurationDocument>.Filter.And(
			Builders<BotConfigurationDocument>.Filter.Eq(b => b.Id, botId),
			Builders<BotConfigurationDocument>.Filter.Eq(b => b.UserId, userId)
		);
		var result = await _dbContext
			.GetCollection<BotConfigurationDocument>()
			.DeleteOneAsync(filter);
		if (result.DeletedCount == 0)
			throw new KeyNotFoundException($"Bot '{botId}' not found for user '{userId}'");
	}
}
