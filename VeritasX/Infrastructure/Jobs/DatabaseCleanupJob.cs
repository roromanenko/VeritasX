using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using VeritasX.Core.Interfaces;
using VeritasX.Core.Options;
using VeritasX.Infrastructure.Persistence.Entities;
using MongoDB.Bson;
using System.Runtime.CompilerServices;
using VeritasX.Core.Domain;

namespace VeritasX.Infrastructure.Jobs;

public class DatabaseCleanupJob : BackgroundService
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<DatabaseCleanupJob> _logger;
	private readonly DatabaseCleanupOptions _options;

	public DatabaseCleanupJob(
		IServiceProvider serviceProvider, 
		ILogger<DatabaseCleanupJob> logger,
		IOptions<DatabaseCleanupOptions> options)
	{
		_serviceProvider = serviceProvider;
		_logger = logger;
		_options = options.Value;
	}

	protected override async Task ExecuteAsync(CancellationToken cancellationToken)
	{
		if (!_options.EnabledCleanupService)
		{
			_logger.LogInformation("Database cleanup service is disabled");
			return;
		}

		_logger.LogInformation("Database cleanup service started. Cleanup interval: {Interval}, Max age: {MaxAge}", 
			_options.CleanupInterval, _options.MaxRecordAge);

		while (!cancellationToken.IsCancellationRequested)
		{
			try
			{
				await CleanupDatabaseAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error during database cleanup");
			}

			await Task.Delay(_options.CleanupInterval, cancellationToken);
		}
	}

	private async Task CleanupDatabaseAsync(CancellationToken cancellationToken)
	{
		using var scope = _serviceProvider.CreateScope();
		var mongoDbContext = scope.ServiceProvider.GetRequiredService<IMongoDbContext>();

		var cutoffDate = DateTime.UtcNow - _options.MaxRecordAge;

		long totalDeletedChunks = 0;
		long totalDeletedJobs = 0;
		int batchNum = 0;

		await foreach (var batch in GetJobIdBatchesAsync(mongoDbContext, cutoffDate, _options.JobBatchSize, cancellationToken))
		{
			batchNum++;
			_logger.LogInformation("Processing batch {BatchNum} with {BatchSize} jobs", batchNum, batch.Count);

			totalDeletedChunks += await CleanupCandleChunksByJobIdsAsync(mongoDbContext, batch, cancellationToken);
			totalDeletedJobs += await CleanupDataCollectionJobsByIdsAsync(mongoDbContext, batch, cancellationToken);
		}

		_logger.LogInformation("Database cleanup completed. Deleted {JobCount} jobs and {ChunkCount} chunks", 
			totalDeletedJobs, totalDeletedChunks);
	}

	private async IAsyncEnumerable<List<ObjectId>> GetJobIdBatchesAsync(IMongoDbContext mongoDbContext, DateTime cutoffDate,
		int batchSize, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var collection = mongoDbContext.GetCollection<DataCollectionJob>("data_collection_jobs");
		
		var filter = Builders<DataCollectionJob>.Filter.And(
			Builders<DataCollectionJob>.Filter.Eq(job => job.State, CollectionState.Completed),
			Builders<DataCollectionJob>.Filter.Lt(job => job.CompletedAt, cutoffDate)
		);

		var totalCount = await collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
		_logger.LogInformation("Found {TotalCount} completed jobs older than {CutoffDate}", totalCount, cutoffDate);

		if (totalCount == 0)
		{
			yield break;
		}

		using var cursor = await collection.Find(filter).Project(job => job.Id).ToCursorAsync(cancellationToken);

		var batch = new List<ObjectId>(batchSize);

		while (await cursor.MoveNextAsync(cancellationToken))
		{
			foreach (var id in cursor.Current)
			{
				batch.Add(id);

				if (batch.Count == batchSize)
				{
					yield return batch;
					batch = new List<ObjectId>(batchSize);
				}
			}
		}
		if (batch.Count > 0)
		{
			yield return batch;
		}
	}
	

	private async Task<long> CleanupDataCollectionJobsByIdsAsync(IMongoDbContext mongoDbContext, List<ObjectId> jobIds, CancellationToken cancellationToken)
	{
		if (!jobIds.Any())
		{
			return 0;
		}

		var collection = mongoDbContext.GetCollection<DataCollectionJob>("data_collection_jobs");
		var filter = Builders<DataCollectionJob>.Filter.In(job => job.Id, jobIds);

		var deleteResult = await collection.DeleteManyAsync(filter, cancellationToken: cancellationToken);

		_logger.LogDebug("Deleted {DeletedCount} data collection jobs", deleteResult.DeletedCount);

		return deleteResult.DeletedCount;
	}

	private async Task<long> CleanupCandleChunksByJobIdsAsync(IMongoDbContext mongoDbContext, List<ObjectId> jobIds, CancellationToken cancellationToken)
	{
		if (!jobIds.Any())
		{
			return 0;
		}

		var collection = mongoDbContext.GetCollection<CandleChunk>("candle_chunks");
		var filter = Builders<CandleChunk>.Filter.In(chunk => chunk.JobId, jobIds);

		var deleteResult = await collection.DeleteManyAsync(filter, cancellationToken: cancellationToken);

		_logger.LogDebug("Deleted {Count} CandleChunk records for {JobCount} jobs", deleteResult.DeletedCount, jobIds.Count);

		return deleteResult.DeletedCount;
	}
}