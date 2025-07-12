using MongoDB.Driver;
using VeritasX.Core.Domain;
using VeritasX.Core.Interfaces;
using VeritasX.Infrastructure.Persistence.Entities;

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

    public async Task<IEnumerable<CandleChunk>> GetChunksByJobIdAsync(Guid jobId)
    {
        var collection = _context.GetCollection<CandleChunk>("candle_chunks");
        return await collection.Find(c => c.JobId == jobId)
            .SortBy(c => c.FromUtc)
            .ToListAsync();
    }

    public async Task<IEnumerable<Candle>> GetCandlesByJobIdAsync(Guid jobId)
    {
        var chunks = await GetChunksByJobIdAsync(jobId);
        return chunks.SelectMany(c => c.Candles).OrderBy(c => c.OpenTime);
    }

    public async Task<bool> ChunkExistsAsync(Guid jobId, DateTime fromUtc, DateTime toUtc)
    {
        var collection = _context.GetCollection<CandleChunk>("candle_chunks");
        return await collection.Find(c => c.JobId == jobId && c.FromUtc == fromUtc && c.ToUtc == toUtc)
            .AnyAsync();
    }

    public async Task DeleteChunksByJobIdAsync(Guid jobId)
    {
        var collection = _context.GetCollection<CandleChunk>("candle_chunks");
        await collection.DeleteManyAsync(c => c.JobId == jobId);
    }
} 