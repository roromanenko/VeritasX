using VeritasX.Core.Domain;
using VeritasX.Infrastructure.Persistence.Entities;
using MongoDB.Bson;

namespace VeritasX.Core.Interfaces;

public interface IDataCollectionService
{
    Task<ObjectId> QueueDataCollectionAsync(string symbol, DateTime fromUtc, DateTime toUtc, TimeSpan interval, ObjectId userId, string? collectionName = null);
    Task<DataCollectionJob?> GetJobAsync(ObjectId jobId);
    Task<IEnumerable<DataCollectionJob>> GetActiveJobsAsync();
    Task<DataCollectionJob?> GetNextPendingJobAsync();
    Task CancelJobAsync(ObjectId jobId);
    Task UpdateJobAsync(DataCollectionJob job);
} 