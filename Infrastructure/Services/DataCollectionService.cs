using AutoMapper;
using Core.Domain;
using Core.Interfaces;
using Core.Options;
using Infrastructure.Persistence.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Services;

public class DataCollectionService : IDataCollectionService
{
	private readonly IMongoDbContext _context;
	private readonly ILogger<DataCollectionService> _logger;
	private readonly IMapper _mapper;
	private readonly ISymbolResolver _symbolResolver;
	private readonly DataCollectorOptions _options;

	public DataCollectionService(
		IMongoDbContext context,
		ILogger<DataCollectionService> logger,
		IOptions<DataCollectorOptions> options,
		IMapper mapper,
		ISymbolResolver symbolResolver)
	{
		_context = context;
		_logger = logger;
		_mapper = mapper;
		_symbolResolver = symbolResolver;
		_options = options.Value;
	}

	public async Task<string> QueueDataCollectionAsync(string symbol, DateTime fromUtc, DateTime toUtc, TimeSpan interval, string userIdStr, string? collectionName = null)
	{
		if (!ObjectId.TryParse(userIdStr, out var userId))
			throw new ArgumentException("Invalid user ID");

		ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(interval, TimeSpan.Zero);

		if (fromUtc >= toUtc)
			throw new ArgumentException("fromUtc must be < toUtc");

		fromUtc = DateTime.SpecifyKind(fromUtc, DateTimeKind.Utc);
		toUtc = DateTime.SpecifyKind(toUtc, DateTimeKind.Utc);
		var symbolInfo = await _symbolResolver.ParseSymbolAsync(symbol);

		var job = new DataCollectionJobDocument
		{
			Symbol = symbol,
			BaseAsset = symbolInfo.BaseAsset,
			QuoteAsset = symbolInfo.QuoteAsset,
			FromUtc = fromUtc,
			ToUtc = toUtc,
			Interval = interval,
			CollectionName = collectionName ?? $"{symbol.ToLowerInvariant()}_{interval.TotalMinutes}m",
			UserId = userId
		};

		// Разбиваем на чанки
		job.Chunks.AddRange(CreateChunks(fromUtc, toUtc, interval));
		job.TotalChunks = job.Chunks.Count;

		// Сохраняем задание в MongoDB
		var collection = _context.GetCollection<DataCollectionJobDocument>("data_collection_jobs");
		await collection.InsertOneAsync(job);

		_logger.LogInformation("Queued data collection job {JobId} for {Symbol} with {ChunkCount} chunks",
			job.Id, job.Symbol, job.TotalChunks);

		return job.Id.ToString();
	}

	public async Task<IEnumerable<DataCollectionJob>> GetJobsAsync(string userIdStr)
	{
		var collection = _context.GetCollection<DataCollectionJobDocument>("data_collection_jobs");

		if (!ObjectId.TryParse(userIdStr, out var userId))
			return [];

		var jobEntities = await collection.Find(j => j.UserId == userId).ToListAsync();
		return _mapper.Map<IEnumerable<DataCollectionJob>>(jobEntities);
	}

	public async Task<DataCollectionJob?> GetJobAsync(string jobIdStr, string userIdStr)
	{
		if (!ObjectId.TryParse(jobIdStr, out var jobId)) return null;
		if (!ObjectId.TryParse(userIdStr, out var userId)) return null;

		var collection = _context.GetCollection<DataCollectionJobDocument>("data_collection_jobs");

		var jobEntity = await collection.Find(j => j.Id == jobId && j.UserId == userId).FirstOrDefaultAsync();
		return _mapper.Map<DataCollectionJob>(jobEntity);
	}

	public async Task<IEnumerable<DataCollectionJob>> GetActiveJobsAsync()
	{
		var docs = await _context.GetCollection<DataCollectionJobDocument>("data_collection_jobs")
		.Find(j => j.State == CollectionState.InProgress || j.State == CollectionState.Pending)
		.ToListAsync();
		return _mapper.Map<IEnumerable<DataCollectionJob>>(docs);
	}

	public async Task<DataCollectionJob?> GetNextPendingJobAsync()
	{
		var doc = await _context.GetCollection<DataCollectionJobDocument>("data_collection_jobs")
								.Find(j => j.State == CollectionState.Pending)
								.SortBy(j => j.CreatedAt)
								.FirstOrDefaultAsync();
		return doc is null ? null : _mapper.Map<DataCollectionJob>(doc);
	}

	public async Task<DataCollectionJob?> GetInterruptedJobAsync(List<string> activeJobIds)
	{
		var parsedIds = activeJobIds.Select(x => ObjectId.Parse(x));
		var doc = await _context.GetCollection<DataCollectionJobDocument>("data_collection_jobs")
								.Find(j => j.State == CollectionState.InProgress
									&& !parsedIds.Contains(j.Id))
								.SortBy(j => j.CreatedAt)
								.FirstOrDefaultAsync();
		return doc is null ? null : _mapper.Map<DataCollectionJob>(doc);
	}

	public async Task CancelJobAsync(string jobIdStr)
	{
		if (!ObjectId.TryParse(jobIdStr, out var jobId))
			throw new ArgumentException("Invalid job ID format");

		var collection = _context.GetCollection<DataCollectionJobDocument>("data_collection_jobs");
		var update = Builders<DataCollectionJobDocument>.Update.Set(j => j.State, CollectionState.Cancelled);
		await collection.UpdateOneAsync(j => j.Id == jobId, update);

		_logger.LogInformation("Cancelled job {JobId}", jobId);
	}

	public async Task UpdateJobAsync(DataCollectionJob job)
	{
		var jobs = _context.GetCollection<DataCollectionJobDocument>("data_collection_jobs");
		var doc = _mapper.Map<DataCollectionJobDocument>(job);
		await jobs.ReplaceOneAsync(j => j.Id == doc.Id, doc);
	}

	private List<DataChunkDocument> CreateChunks(DateTime fromUtc, DateTime toUtc, TimeSpan interval)
	{
		var chunks = new List<DataChunkDocument>();
		var maxCandlesPerChunk = _options.MaxCandlesPerRequest;
		var chunkDuration = TimeSpan.FromTicks(interval.Ticks * maxCandlesPerChunk);

		var current = fromUtc;
		while (current < toUtc)
		{
			var chunkEnd = current.Add(chunkDuration);
			if (chunkEnd > toUtc)
				chunkEnd = toUtc;

			chunks.Add(new DataChunkDocument
			{
				FromUtc = current,
				ToUtc = chunkEnd
			});

			current = chunkEnd;
		}

		return chunks;
	}
}