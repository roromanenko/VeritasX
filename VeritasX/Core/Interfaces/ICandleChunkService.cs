using VeritasX.Core.Domain;
using VeritasX.Infrastructure.Persistence.Entities;
using MongoDB.Bson;

namespace VeritasX.Core.Interfaces;

public interface ICandleChunkService
{
    Task SaveChunkAsync(CandleChunk chunk);
    Task<IEnumerable<CandleChunk>> GetChunksByJobIdAsync(ObjectId jobId);
    Task<IEnumerable<Candle>> GetCandlesByJobIdAsync(ObjectId jobId);
    Task<bool> ChunkExistsAsync(ObjectId jobId, DateTime fromUtc, DateTime toUtc);
    Task DeleteChunksByJobIdAsync(ObjectId jobId);
} 