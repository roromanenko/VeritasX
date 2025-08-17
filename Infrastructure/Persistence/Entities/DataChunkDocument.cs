using Core.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace Infrastructure.Persistence.Entities;

public record DataChunkDocument
{
	public required DateTimeOffset FromUtc { get; init; }
	public required DateTimeOffset ToUtc { get; init; }
	public ChunkState State { get; set; } = ChunkState.Pending;
	public int RetryCount { get; set; }
	public string? ErrorMessage { get; set; }
}