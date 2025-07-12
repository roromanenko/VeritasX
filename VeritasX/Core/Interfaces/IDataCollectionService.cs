using VeritasX.Core.Domain;
using VeritasX.Infrastructure.Persistence.Entities;

namespace VeritasX.Core.Interfaces;

public interface IDataCollectionService
{
    Task<Guid> QueueDataCollectionAsync(string symbol, DateTime fromUtc, DateTime toUtc, TimeSpan interval, string? collectionName = null);
    Task<DataCollectionJob?> GetJobAsync(Guid jobId);
    Task<IEnumerable<DataCollectionJob>> GetActiveJobsAsync();
    Task<DataCollectionJob?> GetNextPendingJobAsync();
    Task CancelJobAsync(Guid jobId);
    Task UpdateJobAsync(DataCollectionJob job);
} 