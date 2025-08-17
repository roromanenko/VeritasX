using MongoDB.Bson;
using MongoDB.Driver;
using Core.Domain;
using Core.Interfaces;
using Core.Constants;
using Infrastructure.Persistence.Entities;
using AutoMapper;

namespace Infrastructure.Services;

public class CandleChunkService(IMongoDbContext context, IMapper mapper) : ICandleChunkService
{
	private readonly IMongoDbContext _context = context;
	private readonly IMapper _mapper = mapper;

	public async Task SaveChunkAsync(CandleChunk chunk)
	{
		var collection = _context.GetCollection<CandleChunkDocument>("candle_chunks");
		var doc = _mapper.Map<CandleChunkDocument>(chunk);

		await collection.InsertOneAsync(doc);
	}

	public async Task<IEnumerable<CandleChunk>> GetChunksByJobIdAsync(string jobIdStr)
	{
		var jobId = _mapper.Map<ObjectId>(jobIdStr);

		var collection = _context.GetCollection<CandleChunkDocument>("candle_chunks");
		var docs = await collection.Find(d => d.JobId == jobId)
									.SortBy(d => d.FromUtc)
									.ToListAsync();

		return _mapper.Map<IEnumerable<CandleChunk>>(docs);
	}

	public async Task<IEnumerable<Candle>> GetCandlesByJobIdAsync(string jobIdStr, string userIdStr, string userRole)
	{
		if (!ObjectId.TryParse(jobIdStr, out var jobId))
			return [];
		if (!ObjectId.TryParse(userIdStr, out var userId))
			return [];

		if (userRole != AppRoles.Admin)
		{
			var jobs = _context.GetCollection<DataCollectionJobDocument>("data_collection_jobs");
			var job = await jobs.Find(j => j.Id == jobId && j.UserId == userId).FirstOrDefaultAsync();
			if (job is null)
				return [];
		}

		var chunks = await GetChunksByJobIdAsync(jobIdStr);
		return chunks.SelectMany(c => c.Candles).OrderBy(c => c.OpenTimeUtc);
	}

	public async Task<bool> ChunkExistsAsync(string jobIdStr, DateTime fromUtc, DateTime toUtc)
	{
		if (!ObjectId.TryParse(jobIdStr, out var jobId))
			return false;

		fromUtc = DateTime.SpecifyKind(fromUtc, DateTimeKind.Utc);
		toUtc = DateTime.SpecifyKind(toUtc, DateTimeKind.Utc);

		var collection = _context.GetCollection<CandleChunkDocument>("candle_chunks");
		return await collection.Find(c => c.JobId == jobId
										&& c.FromUtc == fromUtc
										&& c.ToUtc == toUtc)
										.AnyAsync();
	}

	public async Task DeleteChunksByJobIdAsync(string jobIdStr)
	{
		var jobId = _mapper.Map<ObjectId>(jobIdStr);

		var collection = _context.GetCollection<CandleChunkDocument>("candle_chunks");
		await collection.DeleteManyAsync(c => c.JobId == jobId);
	}
}