using Core.Domain;
using MongoDB.Bson;

namespace Core.Interfaces;

public interface IDataCollectionService
{
	Task<string> QueueDataCollectionAsync(string symbol, DateTime fromUtc, DateTime toUtc, TimeSpan interval, string userIdStr, string? collectionName = null);
	Task<IEnumerable<DataCollectionJob>> GetJobsAsync(string userIdStr);
	Task<DataCollectionJob?> GetJobAsync(string jobIdStr, string userIdStr);
	Task<IEnumerable<DataCollectionJob>> GetActiveJobsAsync();
	Task<DataCollectionJob?> GetNextPendingJobAsync();
	Task CancelJobAsync(string jobIdStr);
	Task UpdateJobAsync(DataCollectionJob job);
}