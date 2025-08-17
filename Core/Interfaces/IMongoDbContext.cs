using MongoDB.Driver;

namespace Core.Interfaces;

public interface IMongoDbContext
{
	IMongoCollection<T> GetCollection<T>(string collectionName);
	IMongoCollection<T> GetCollection<T>();
	Task<bool> CollectionExistsAsync(string collectionName);
	Task CreateCollectionAsync(string collectionName);
	Task DropCollectionAsync(string collectionName);
}