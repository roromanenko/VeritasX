using VeritasX.Core.Interfaces;
using VeritasX.Infrastructure.Persistence.Entities;
using VeritasX.Infrastructure.Persistence.MongoDb;
using VeritasX.Core.Options;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;

namespace VeritasX.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
	private readonly MongoDbOptions _mongoDbOptions;
	private readonly IMongoDbContext _dbContext;

	public UserRepository(IOptions<MongoDbOptions> mongoDbOptions, IMongoDbContext dbContext)
	{
		_mongoDbOptions = mongoDbOptions.Value;
		_dbContext = dbContext;
	}

	public async Task<User> CreateUser(User newUser)
	{
		await _dbContext.GetCollection<User>().InsertOneAsync(newUser);
		return newUser;
	}

	public async Task<User> GetUserByUsername(string username)
	{
		var filter = Builders<User>.Filter.Eq(u => u.Username, username);
		var user = await _dbContext.GetCollection<User>().Find(filter).FirstOrDefaultAsync();
		return user;
	}

	public async Task<User> GetUserById(ObjectId userid)
	{
		var filter = Builders<User>.Filter.Eq(u => u.Id, userid);
		var user = await _dbContext.GetCollection<User>().Find(filter).FirstOrDefaultAsync();
		return user;
	}

	public Task UpdateUser(User user)
	{
		var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);

		return _dbContext.GetCollection<User>().ReplaceOneAsync(filter, user);
	}

	public Task ChangePassword(ObjectId userId, string newPasswordHash)
	{
		var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
		var update = Builders<User>.Update.Set(x => x.PasswordHash, newPasswordHash);

		return _dbContext.GetCollection<User>().UpdateOneAsync(filter, update);
	}

	public async Task DeleteUser(ObjectId userId)
	{
		var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
		await _dbContext.GetCollection<User>().DeleteOneAsync(filter);
	}

	public async Task<IEnumerable<User>> GetAllUsers()
	{
		return await _dbContext.GetCollection<User>().Find(_ => true).ToListAsync();
	}

	public async Task<bool> UserExistsByUsername(string username)
	{
		var filter = Builders<User>.Filter.Eq(u => u.Username, username);
		var count = await _dbContext.GetCollection<User>().CountDocumentsAsync(filter);
		return count > 0;
	}
}