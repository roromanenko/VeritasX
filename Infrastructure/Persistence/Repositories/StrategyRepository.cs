using Core.Interfaces;
using Infrastructure.Interfaces;
using Infrastructure.Persistence.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Persistence.Repositories;

public class StrategyRepository : IStrategyRepository
{
	private readonly IMongoDbContext _dbContext;

	public StrategyRepository(IMongoDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<StrategyDocument?> GetById(ObjectId id)
	{
		var filter = Builders<StrategyDocument>.Filter.Eq(s => s.Id, id);
		return await _dbContext
			.GetCollection<StrategyDocument>()
			.Find(filter)
			.FirstOrDefaultAsync();
	}

	public async Task<IEnumerable<StrategyDocument>> GetPublic()
	{
		var filter = Builders<StrategyDocument>.Filter.Eq(s => s.IsPublic, true);
		return await _dbContext
			.GetCollection<StrategyDocument>()
			.Find(filter)
			.ToListAsync();
	}

	public async Task<IEnumerable<StrategyDocument>> GetByAuthor(ObjectId authorId)
	{
		var filter = Builders<StrategyDocument>.Filter.Eq(s => s.AuthorId, authorId);
		return await _dbContext
			.GetCollection<StrategyDocument>()
			.Find(filter)
			.ToListAsync();
	}

	public async Task<StrategyDocument> Create(StrategyDocument strategy)
	{
		await _dbContext
			.GetCollection<StrategyDocument>()
			.InsertOneAsync(strategy);
		return strategy;
	}

	public Task Update(StrategyDocument strategy)
	{
		var filter = Builders<StrategyDocument>.Filter.Eq(s => s.Id, strategy.Id);
		return _dbContext
			.GetCollection<StrategyDocument>()
			.ReplaceOneAsync(filter, strategy);
	}

	public async Task Delete(ObjectId id)
	{
		var filter = Builders<StrategyDocument>.Filter.Eq(s => s.Id, id);
		var result = await _dbContext
			.GetCollection<StrategyDocument>()
			.DeleteOneAsync(filter);
		if (result.DeletedCount == 0)
			throw new KeyNotFoundException($"Strategy '{id}' not found");
	}
}
