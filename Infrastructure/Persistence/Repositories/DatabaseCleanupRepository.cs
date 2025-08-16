using Core.Interfaces;
using Core.Options;
using Infrastructure.Persistence.Entities;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Runtime.CompilerServices;

namespace Infrastructure.Persistence.Repositories;

public class DatabaseCleanupRepository : IDatabaseCleanupRepository
{
	private readonly MongoDbOptions _mongoDbOptions;
	private readonly IMongoDbContext _dbContext;

	public DatabaseCleanupRepository(IOptions<MongoDbOptions> mongoDbOptions, IMongoDbContext dbContext)
	{
		_mongoDbOptions = mongoDbOptions.Value;
		_dbContext = dbContext;
	}

	public async Task<long> GetCompletedJobCountAsync(DateTime cutoffDate, CancellationToken cancellationToken)
	{
		var filter = Builders<DataCollectionJobDocument>.Filter.And(Builders<DataCollectionJobDocument>.Filter.Lt(job => job.CompletedAt, cutoffDate));

		return await _dbContext
			.GetCollection<DataCollectionJobDocument>("data_collection_jobs")
			.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
	}

	public async IAsyncEnumerable<IEnumerable<ObjectId>> GetJobIdBatchesAsync(
		DateTime cutoffDate,
		int batchSize,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var filter = Builders<DataCollectionJobDocument>.Filter.And(Builders<DataCollectionJobDocument>.Filter.Lt(job => job.CompletedAt, cutoffDate));

		using var cursor = await _dbContext
			.GetCollection<DataCollectionJobDocument>("data_collection_jobs")
			.Find(filter, new() { BatchSize = batchSize })
			.Project(job => job.Id)
			.ToCursorAsync(cancellationToken);

		while (await cursor.MoveNextAsync(cancellationToken))
		{
			yield return cursor.Current;
		}
	}

	public async Task<long> CleanupDataCollectionJobsByIdsAsync(IEnumerable<ObjectId> jobIds, CancellationToken cancellationToken)
	{
		if (!jobIds.Any())
		{
			return 0;
		}

		var filter = Builders<DataCollectionJobDocument>.Filter.In(job => job.Id, jobIds);

		var deletedCount = await _dbContext
			.GetCollection<DataCollectionJobDocument>("data_collection_jobs")
			.DeleteManyAsync(filter, cancellationToken: cancellationToken);

		return deletedCount.DeletedCount;
	}

	public async Task<long> CleanupCandleChunksByJobIdsAsync(IEnumerable<ObjectId> jobIds, CancellationToken cancellationToken)
	{
		if (!jobIds.Any())
		{
			return 0;
		}

		var filter = Builders<CandleChunkDocument>.Filter.In(chunk => chunk.JobId, jobIds);

		var deletedCount = await _dbContext
			.GetCollection<CandleChunkDocument>("candle_chunks")
			.DeleteManyAsync(filter, cancellationToken: cancellationToken);

		return deletedCount.DeletedCount;
	}
}