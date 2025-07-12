using VeritasX.Core.Domain;
using VeritasX.Infrastructure.Persistence.Entities;

namespace VeritasX.Core.Interfaces;

public interface ICandleChunkService
{
    Task SaveChunkAsync(CandleChunk chunk);
    Task<IEnumerable<CandleChunk>> GetChunksByJobIdAsync(Guid jobId);
    Task<IEnumerable<Candle>> GetCandlesByJobIdAsync(Guid jobId);
    Task<bool> ChunkExistsAsync(Guid jobId, DateTime fromUtc, DateTime toUtc);
    Task DeleteChunksByJobIdAsync(Guid jobId);
} 