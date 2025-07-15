using VeritasX.Core.Domain;
using VeritasX.Infrastructure.Persistence.Entities;
using MongoDB.Bson;

namespace VeritasX.Core.Interfaces;

public interface ICandleChunkService
{
    Task SaveChunkAsync(CandleChunk chunk);
    Task<IEnumerable<CandleChunk>> GetChunksByJobIdAsync(string jobIdStr);
    Task<IEnumerable<Candle>> GetCandlesByJobIdAsync(string jobIdStr, string userIdStr, string userRole);
    Task<bool> ChunkExistsAsync(string jobIdStr, DateTime fromUtc, DateTime toUtc);
    Task DeleteChunksByJobIdAsync(string jobIdStr);
} 