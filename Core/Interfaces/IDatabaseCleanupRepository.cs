using MongoDB.Bson;

namespace Core.Interfaces;

public interface IDatabaseCleanupRepository
{
	Task<long> GetCompletedJobCountAsync(DateTime cutoffDate, CancellationToken cancellationToken);
	IAsyncEnumerable<IEnumerable<ObjectId>> GetJobIdBatchesAsync(DateTime cutoffDate, int batchSize, CancellationToken cancellationToken);
	Task<long> CleanupDataCollectionJobsByIdsAsync(IEnumerable<ObjectId> jobIds, CancellationToken cancellationToken);
	Task<long> CleanupCandleChunksByJobIdsAsync(IEnumerable<ObjectId> jobIds, CancellationToken cancellationToken);
}