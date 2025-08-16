using Core.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace Infrastructure.Persistence.Entities;

public record DataChunkDocument
{
	[BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
	public required DateTime FromUtc { get; init; }

	[BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
	public required DateTime ToUtc { get; init; }
	public ChunkState State { get; set; } = ChunkState.Pending;
	public int RetryCount { get; set; }
	public string? ErrorMessage { get; set; }
}