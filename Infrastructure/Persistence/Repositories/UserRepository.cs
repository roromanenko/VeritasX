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

	public async Task<UserEntity> CreateUser(UserEntity newUser)
	{
		await _dbContext
			.GetCollection<UserEntity>()
			.InsertOneAsync(newUser);

		return newUser;
	}

	public async Task<UserEntity> GetUserByUsername(string username)
	{
		var filter = Builders<UserEntity>.Filter.Eq(u => u.Username, username);

		var user = await _dbContext
			.GetCollection<UserEntity>()
			.Find(filter)
			.FirstOrDefaultAsync();

		return user;
	}

	public async Task<UserEntity> GetUserById(ObjectId userid)
	{
		var filter = Builders<UserEntity>.Filter.Eq(u => u.Id, userid);

		var user = await _dbContext
			.GetCollection<UserEntity>()
			.Find(filter)
			.FirstOrDefaultAsync();

		return user;
	}

	public Task UpdateUser(UserEntity user)
	{
		var filter = Builders<UserEntity>.Filter.Eq(u => u.Id, user.Id);

		return _dbContext
			.GetCollection<UserEntity>()
			.ReplaceOneAsync(filter, user);
	}

	public Task ChangePassword(ObjectId userId, string newPasswordHash)
	{
		var filter = Builders<UserEntity>.Filter.Eq(u => u.Id, userId);

		var update = Builders<UserEntity>.Update.Set(x => x.PasswordHash, newPasswordHash);

		return _dbContext
			.GetCollection<UserEntity>()
			.UpdateOneAsync(filter, update);
	}

	public async Task DeleteUser(ObjectId userId)
	{
		var filter = Builders<UserEntity>.Filter.Eq(u => u.Id, userId);

		await _dbContext
			.GetCollection<UserEntity>()
			.DeleteOneAsync(filter);
	}

	#endregion

	#region Exchange connection actions

	public async Task AddExchangeConnection(ObjectId userId, ExchangeName exchange, ExchangeConnectionEntity connection)
	{
		var userFilter = Builders<UserEntity>.Filter.Eq(u => u.Id, userId);
		var existsFilter = Builders<UserEntity>.Filter.And(
			userFilter,
			Builders<UserEntity>.Filter.Exists($"ExchangeConnections.{exchange}")
		);

		var alreadyExists = await _dbContext
			.GetCollection<UserEntity>()
			.CountDocumentsAsync(existsFilter) > 0;

		if (alreadyExists)
			throw new InvalidOperationException($"Connection for exchange '{exchange}' already exists for user '{userId}'");

		var update = Builders<UserEntity>.Update.Set($"ExchangeConnections.{exchange}", connection);
		await _dbContext
			.GetCollection<UserEntity>()
			.UpdateOneAsync(userFilter, update);
	}

	public async Task UpdateExchangeConnection(ObjectId userId, ExchangeName exchange, ExchangeConnectionEntity connection)
	{
		var filter = Builders<UserEntity>.Filter.And(
			Builders<UserEntity>.Filter.Eq(u => u.Id, userId),
			Builders<UserEntity>.Filter.Exists($"ExchangeConnections.{exchange}")
		);

		var update = Builders<UserEntity>.Update.Set($"ExchangeConnections.{exchange}", connection);

		var result = await _dbContext
			.GetCollection<UserEntity>()
			.UpdateOneAsync(filter, update);

		if (result.MatchedCount == 0)
			throw new KeyNotFoundException($"Connection for exchange '{exchange}' not found for user '{userId}'");
	}

	public async Task RemoveExchangeConnection(ObjectId userId, ExchangeName exchange)
	{
		var filter = Builders<UserEntity>.Filter.Eq(u => u.Id, userId);
		var update = Builders<UserEntity>.Update.Unset($"ExchangeConnections.{exchange}");

		var result = await _dbContext
			.GetCollection<UserEntity>()
			.UpdateOneAsync(filter, update);

		if (result.MatchedCount == 0)
			throw new KeyNotFoundException($"Connection for exchange '{exchange}' not found for user '{userId}'");
	}

	public async Task<ExchangeConnectionEntity> GetExchangeConnection(ObjectId userId, ExchangeName exchange)
	{
		var filter = Builders<UserEntity>.Filter.Eq(u => u.Id, userId);

		var user = await _dbContext
			.GetCollection<UserEntity>()
			.Find(filter)
			.FirstOrDefaultAsync();

		if (user?.ExchangeConnections == null || !user.ExchangeConnections.TryGetValue(exchange, out var connection))
			throw new KeyNotFoundException($"Connection for exchange '{exchange}' not found for user '{userId}'");

		return connection;
	}

	public async Task<Dictionary<ExchangeName, ExchangeConnectionEntity>> GetAllExchangeConnections(ObjectId userId)
	{
		var filter = Builders<UserEntity>.Filter.Eq(u => u.Id, userId);

		var user = await _dbContext
			.GetCollection<UserEntity>()
			.Find(filter)
			.FirstOrDefaultAsync();

		if (user?.ExchangeConnections == null || user.ExchangeConnections.Count == 0)
			throw new KeyNotFoundException($"No exchange connections found for user '{userId}'");

		return user.ExchangeConnections;
	}

	#endregion
}
