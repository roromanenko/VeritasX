using Core.Interfaces;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.MongoDb;
using Core.Options;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using Infrastructure.Interfaces;

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

	public async Task<UserEntity> CreateUser(UserEntity newUser)
	{
		await _dbContext.GetCollection<UserEntity>().InsertOneAsync(newUser);
		return newUser;
	}

	public async Task<UserEntity> GetUserByUsername(string username)
	{
		var filter = Builders<UserEntity>.Filter.Eq(u => u.Username, username);
		var user = await _dbContext.GetCollection<UserEntity>().Find(filter).FirstOrDefaultAsync();
		return user;
	}

	public async Task<UserEntity> GetUserById(ObjectId userid)
	{
		var filter = Builders<UserEntity>.Filter.Eq(u => u.Id, userid);
		var user = await _dbContext.GetCollection<UserEntity>().Find(filter).FirstOrDefaultAsync();
		return user;
	}

	public Task UpdateUser(UserEntity user)
	{
		var filter = Builders<UserEntity>.Filter.Eq(u => u.Id, user.Id);

		return _dbContext.GetCollection<UserEntity>().ReplaceOneAsync(filter, user);
	}

	public Task ChangePassword(ObjectId userId, string newPasswordHash)
	{
		var filter = Builders<UserEntity>.Filter.Eq(u => u.Id, userId);
		var update = Builders<UserEntity>.Update.Set(x => x.PasswordHash, newPasswordHash);

		return _dbContext.GetCollection<UserEntity>().UpdateOneAsync(filter, update);
	}

	public async Task DeleteUser(ObjectId userId)
	{
		var filter = Builders<UserEntity>.Filter.Eq(u => u.Id, userId);
		await _dbContext.GetCollection<UserEntity>().DeleteOneAsync(filter);
	}

	public async Task<IEnumerable<UserEntity>> GetAllUsers()
	{
		return await _dbContext.GetCollection<UserEntity>().Find(_ => true).ToListAsync();
	}

	public async Task<bool> UserExistsByUsername(string username)
	{
		var filter = Builders<UserEntity>.Filter.Eq(u => u.Username, username);
		var count = await _dbContext.GetCollection<UserEntity>().CountDocumentsAsync(filter);
		return count > 0;
	}
}