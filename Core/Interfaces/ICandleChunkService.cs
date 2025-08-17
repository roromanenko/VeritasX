using Core.Domain;

namespace Core.Interfaces;

public interface ICandleChunkService
{
	Task SaveChunkAsync(CandleChunk chunk);
	Task<IEnumerable<CandleChunk>> GetChunksByJobIdAsync(string jobIdStr);
	Task<IEnumerable<Candle>> GetCandlesByJobIdAsync(string jobIdStr);
	Task<bool> ChunkExistsAsync(string jobIdStr, DateTime fromUtc, DateTime toUtc);
	Task DeleteChunksByJobIdAsync(string jobIdStr);
}