using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using VeritasX.Core.Interfaces;
using VeritasX.Core.Options;

namespace VeritasX.Infrastructure.Persistence.MongoDb;

public class MongoDbContext : IMongoDbContext
{
	private readonly IMongoDatabase _database;
	private readonly MongoDbOptions _options;

	static MongoDbContext()
	{
		// Конвенции для имен полей
		var pack = new ConventionPack
		{
			new CamelCaseElementNameConvention()
		};
		ConventionRegistry.Register("camelCase", pack, t => true);
	}

	public MongoDbContext(IMongoClient client, IOptions<MongoDbOptions> options)
	{
		_options = options.Value;
		_database = client.GetDatabase(_options.DatabaseName);
	}

	public IMongoCollection<T> GetCollection<T>(string collectionName)
	{
		return _database.GetCollection<T>(collectionName);
	}

	public IMongoCollection<T> GetCollection<T>()
	{
		var collectionName = typeof(T).Name.ToLowerInvariant();
		return GetCollection<T>(collectionName);
	}

	public async Task<bool> CollectionExistsAsync(string collectionName)
	{
		var filter = new BsonDocument("name", collectionName);
		var collections = await _database.ListCollectionNamesAsync(new ListCollectionNamesOptions { Filter = filter });
		return await collections.AnyAsync();
	}

	public async Task CreateCollectionAsync(string collectionName)
	{
		if (!await CollectionExistsAsync(collectionName))
		{
			await _database.CreateCollectionAsync(collectionName);
		}
	}

	public async Task DropCollectionAsync(string collectionName)
	{
		if (await CollectionExistsAsync(collectionName))
		{
			await _database.DropCollectionAsync(collectionName);
		}
	}
}