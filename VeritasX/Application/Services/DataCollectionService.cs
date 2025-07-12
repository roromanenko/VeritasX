using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using VeritasX.Core.Domain;
using VeritasX.Core.Options;
using VeritasX.Core.Interfaces;
using VeritasX.Infrastructure.Persistence.Entities;

namespace VeritasX.Application.Services;

public class DataCollectionService : IDataCollectionService
{
    private readonly IMongoDbContext _context;
    private readonly ILogger<DataCollectionService> _logger;
    private readonly DataCollectorOptions _options;

    public DataCollectionService(
        IMongoDbContext context,
        ILogger<DataCollectionService> logger,
        IOptions<DataCollectorOptions> options)
    {
        _context = context;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<Guid> QueueDataCollectionAsync(string symbol, DateTime fromUtc, DateTime toUtc, TimeSpan interval, string? collectionName = null)
    {
        var job = new DataCollectionJob
        {
            Symbol = symbol,
            FromUtc = fromUtc,
            ToUtc = toUtc,
            Interval = interval,
            CollectionName = collectionName ?? $"{symbol.ToLower()}_{interval.TotalMinutes}m"
        };

        // Разбиваем на чанки
        job.Chunks.AddRange(CreateChunks(fromUtc, toUtc, interval));
        job.TotalChunks = job.Chunks.Count;

        // Сохраняем задание в MongoDB
        var collection = _context.GetCollection<DataCollectionJob>("data_collection_jobs");
        await collection.InsertOneAsync(job);

        _logger.LogInformation("Queued data collection job {JobId} for {Symbol} with {ChunkCount} chunks", 
            job.Id, job.Symbol, job.TotalChunks);

        return job.Id;
    }

    public async Task<DataCollectionJob?> GetJobAsync(Guid jobId)
    {
        var collection = _context.GetCollection<DataCollectionJob>("data_collection_jobs");
        return await collection.Find(j => j.Id == jobId).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<DataCollectionJob>> GetActiveJobsAsync()
    {
        var collection = _context.GetCollection<DataCollectionJob>("data_collection_jobs");
        return await collection.Find(j => j.State == CollectionState.InProgress || j.State == CollectionState.Pending)
            .ToListAsync();
    }

    public async Task<DataCollectionJob?> GetNextPendingJobAsync()
    {
        var collection = _context.GetCollection<DataCollectionJob>("data_collection_jobs");
        return await collection.Find(j => j.State == CollectionState.Pending)
            .SortBy(j => j.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task CancelJobAsync(Guid jobId)
    {
        var collection = _context.GetCollection<DataCollectionJob>("data_collection_jobs");
        var update = Builders<DataCollectionJob>.Update.Set(j => j.State, CollectionState.Cancelled);
        await collection.UpdateOneAsync(j => j.Id == jobId, update);
        
        _logger.LogInformation("Cancelled job {JobId}", jobId);
    }

    public async Task UpdateJobAsync(DataCollectionJob job)
    {
        var collection = _context.GetCollection<DataCollectionJob>("data_collection_jobs");
        await collection.ReplaceOneAsync(j => j.Id == job.Id, job);
    }

    private List<DataChunk> CreateChunks(DateTime fromUtc, DateTime toUtc, TimeSpan interval)
    {
        var chunks = new List<DataChunk>();
        var maxCandlesPerChunk = _options.MaxCandlesPerRequest;
        var chunkDuration = TimeSpan.FromTicks(interval.Ticks * maxCandlesPerChunk);
        
        var current = fromUtc;
        while (current < toUtc)
        {
            var chunkEnd = current.Add(chunkDuration);
            if (chunkEnd > toUtc)
                chunkEnd = toUtc;
                
            chunks.Add(new DataChunk
            {
                FromUtc = current,
                ToUtc = chunkEnd
            });
            
            current = chunkEnd;
        }
        
        return chunks;
    }
} 