using Core.Domain;
using Core.Interfaces;
using Core.Options;
using Infrastructure.Interfaces;
using Infrastructure.Persistence.Entities;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
	private readonly MongoDbOptions _mongoDbOptions;
	private readonly IMongoDbContext _dbContext;

	public UserRepository(IOptions<MongoDbOptions> mongoDbOptions, IMongoDbContext dbContext)
	{
		_mongoDbOptions = mongoDbOptions.Value;
		_dbContext = dbContext;
	}

	#region User actions

	public async Task<UserDocument> CreateUser(UserDocument newUser)
	{
		await _dbContext
			.GetCollection<UserDocument>()
			.InsertOneAsync(newUser);

		return newUser;
	}

	public async Task<UserDocument> GetUserByUsername(string username)
	{
		var filter = Builders<UserDocument>.Filter.Eq(u => u.Username, username);

		var user = await _dbContext
			.GetCollection<UserDocument>()
			.Find(filter)
			.FirstOrDefaultAsync();

		return user;
	}

	public async Task<UserDocument> GetUserById(ObjectId userid)
	{
		var filter = Builders<UserDocument>.Filter.Eq(u => u.Id, userid);

		var user = await _dbContext
			.GetCollection<UserDocument>()
			.Find(filter)
			.FirstOrDefaultAsync();

		return user;
	}

	public Task UpdateUser(UserDocument user)
	{
		var filter = Builders<UserDocument>.Filter.Eq(u => u.Id, user.Id);

		return _dbContext
			.GetCollection<UserDocument>()
			.ReplaceOneAsync(filter, user);
	}

	public Task ChangePassword(ObjectId userId, string newPasswordHash)
	{
		var filter = Builders<UserDocument>.Filter.Eq(u => u.Id, userId);

		var update = Builders<UserDocument>.Update.Set(x => x.PasswordHash, newPasswordHash);

		return _dbContext
			.GetCollection<UserDocument>()
			.UpdateOneAsync(filter, update);
	}

	public async Task DeleteUser(ObjectId userId)
	{
		var filter = Builders<UserDocument>.Filter.Eq(u => u.Id, userId);

		await _dbContext
			.GetCollection<UserDocument>()
			.DeleteOneAsync(filter);
	}

	#endregion

	#region Exchange connection actions

	public async Task AddExchangeConnection(ObjectId userId, ExchangeName exchange, ExchangeConnectionDocument connection)
	{
		var userFilter = Builders<UserDocument>.Filter.Eq(u => u.Id, userId);
		var existsFilter = Builders<UserDocument>.Filter.And(
			userFilter,
			Builders<UserDocument>.Filter.Exists($"ExchangeConnections.{exchange}")
		);

		var alreadyExists = await _dbContext
			.GetCollection<UserDocument>()
			.CountDocumentsAsync(existsFilter) > 0;

		if (alreadyExists)
			throw new InvalidOperationException($"Connection for exchange '{exchange}' already exists for user '{userId}'");

		var update = Builders<UserDocument>.Update.Set($"ExchangeConnections.{exchange}", connection);
		await _dbContext
			.GetCollection<UserDocument>()
			.UpdateOneAsync(userFilter, update);
	}

	public async Task UpdateExchangeConnection(ObjectId userId, ExchangeName exchange, ExchangeConnectionDocument connection)
	{
		var filter = Builders<UserDocument>.Filter.And(
			Builders<UserDocument>.Filter.Eq(u => u.Id, userId),
			Builders<UserDocument>.Filter.Exists($"ExchangeConnections.{exchange}")
		);

		var update = Builders<UserDocument>.Update.Set($"ExchangeConnections.{exchange}", connection);

		var result = await _dbContext
			.GetCollection<UserDocument>()
			.UpdateOneAsync(filter, update);

		if (result.MatchedCount == 0)
			throw new KeyNotFoundException($"Connection for exchange '{exchange}' not found for user '{userId}'");
	}

	public async Task RemoveExchangeConnection(ObjectId userId, ExchangeName exchange)
	{
		var filter = Builders<UserDocument>.Filter.Eq(u => u.Id, userId);
		var update = Builders<UserDocument>.Update.Unset($"ExchangeConnections.{exchange}");

		var result = await _dbContext
			.GetCollection<UserDocument>()
			.UpdateOneAsync(filter, update);

		if (result.MatchedCount == 0)
			throw new KeyNotFoundException($"Connection for exchange '{exchange}' not found for user '{userId}'");
	}

	public async Task<ExchangeConnectionDocument> GetExchangeConnection(ObjectId userId, ExchangeName exchange)
	{
		var filter = Builders<UserDocument>.Filter.Eq(u => u.Id, userId);

		var user = await _dbContext
			.GetCollection<UserDocument>()
			.Find(filter)
			.FirstOrDefaultAsync();

		if (user?.ExchangeConnections == null || !user.ExchangeConnections.TryGetValue(exchange, out var connection))
			throw new KeyNotFoundException($"Connection for exchange '{exchange}' not found for user '{userId}'");

		return connection;
	}

	public async Task<Dictionary<ExchangeName, ExchangeConnectionDocument>> GetAllExchangeConnections(ObjectId userId)
	{
		var filter = Builders<UserDocument>.Filter.Eq(u => u.Id, userId);

		var user = await _dbContext
			.GetCollection<UserDocument>()
			.Find(filter)
			.FirstOrDefaultAsync();

		if (user?.ExchangeConnections == null || user.ExchangeConnections.Count == 0)
			throw new KeyNotFoundException($"No exchange connections found for user '{userId}'");

		return user.ExchangeConnections;
	}

	#endregion
}
