using MongoDB.Bson;
using MongoDB.Driver;
using VeritasX.Core.Domain;
using VeritasX.Core.Interfaces;
using VeritasX.Infrastructure.Persistence.Entities;
using VeritasX.Core.Constants;

namespace VeritasX.Application.Services;

public class CandleChunkService : ICandleChunkService
{
	private readonly IMongoDbContext _context;

	public CandleChunkService(IMongoDbContext context)
	{
		_context = context;
	}

	public async Task SaveChunkAsync(CandleChunk chunk)
	{
		var collection = _context.GetCollection<CandleChunk>("candle_chunks");
		await collection.InsertOneAsync(chunk);
	}

	public async Task<IEnumerable<CandleChunk>> GetChunksByJobIdAsync(string jobIdStr)
	{
		if (!ObjectId.TryParse(jobIdStr, out var jobId))
			throw new ArgumentException("Invalid job ID");

		var collection = _context.GetCollection<CandleChunk>("candle_chunks");
		return await collection.Find(c => c.JobId == jobId)
			.SortBy(c => c.FromUtc)
			.ToListAsync();
	}

	public async Task<IEnumerable<Candle>> GetCandlesByJobIdAsync(string jobIdStr, string userIdStr, string userRole)
	{
		if (!ObjectId.TryParse(jobIdStr, out var jobId))
			throw new ArgumentException("Invalid job ID");

		if (!ObjectId.TryParse(userIdStr, out var userId))
			throw new ArgumentException("Invalid user ID");

		if (userRole != AppRoles.Admin)
		{
			var job = await _context.GetCollection<DataCollectionJob>("data_collection_jobs")
				.Find(j => j.Id == jobId && j.UserId == userId)
				.FirstOrDefaultAsync();
			if (job == null)
				return Enumerable.Empty<Candle>();
		}

		var chunks = await GetChunksByJobIdAsync(jobIdStr);
		return chunks.SelectMany(c => c.Candles).OrderBy(c => c.OpenTime);
	}

	public async Task<bool> ChunkExistsAsync(string jobIdStr, DateTime fromUtc, DateTime toUtc)
	{
		if (!ObjectId.TryParse(jobIdStr, out var jobId))
			throw new ArgumentException("Invalid job ID");

		var collection = _context.GetCollection<CandleChunk>("candle_chunks");
		return await collection.Find(c => c.JobId == jobId && c.FromUtc == fromUtc && c.ToUtc == toUtc)
			.AnyAsync();
	}

	public async Task DeleteChunksByJobIdAsync(string jobIdStr)
	{
		if (!ObjectId.TryParse(jobIdStr, out var jobId))
			throw new ArgumentException("Invalid job ID");

		var collection = _context.GetCollection<CandleChunk>("candle_chunks");
		await collection.DeleteManyAsync(c => c.JobId == jobId);
	}
}