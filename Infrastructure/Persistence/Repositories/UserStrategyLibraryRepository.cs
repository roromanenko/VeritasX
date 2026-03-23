using Core.Interfaces;
using Infrastructure.Interfaces;
using Infrastructure.Persistence.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Persistence.Repositories;

public class UserStrategyLibraryRepository : IUserStrategyLibraryRepository
{
	private readonly IMongoDbContext _dbContext;

	public UserStrategyLibraryRepository(IMongoDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<IEnumerable<UserStrategyLibraryDocument>> GetByUserId(ObjectId userId)
	{
		var filter = Builders<UserStrategyLibraryDocument>.Filter.Eq(e => e.UserId, userId);
		return await _dbContext
			.GetCollection<UserStrategyLibraryDocument>()
			.Find(filter)
			.ToListAsync();
	}

	public async Task<UserStrategyLibraryDocument?> GetByUserAndStrategy(ObjectId userId, ObjectId strategyId)
	{
		var filter = Builders<UserStrategyLibraryDocument>.Filter.And(
			Builders<UserStrategyLibraryDocument>.Filter.Eq(e => e.UserId, userId),
			Builders<UserStrategyLibraryDocument>.Filter.Eq(e => e.StrategyId, strategyId)
		);
		return await _dbContext
			.GetCollection<UserStrategyLibraryDocument>()
			.Find(filter)
			.FirstOrDefaultAsync();
	}

	public async Task<UserStrategyLibraryDocument> Create(UserStrategyLibraryDocument entry)
	{
		await _dbContext
			.GetCollection<UserStrategyLibraryDocument>()
			.InsertOneAsync(entry);
		return entry;
	}

	public Task Update(UserStrategyLibraryDocument entry)
	{
		var filter = Builders<UserStrategyLibraryDocument>.Filter.Eq(e => e.Id, entry.Id);
		return _dbContext
			.GetCollection<UserStrategyLibraryDocument>()
			.ReplaceOneAsync(filter, entry);
	}

	public async Task Delete(ObjectId id)
	{
		var filter = Builders<UserStrategyLibraryDocument>.Filter.Eq(e => e.Id, id);
		var result = await _dbContext
			.GetCollection<UserStrategyLibraryDocument>()
			.DeleteOneAsync(filter);
		if (result.DeletedCount == 0)
			throw new KeyNotFoundException($"UserStrategyLibrary entry '{id}' not found");
	}
}
