using Core.Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Infrastructure.Persistence.Entities;

public record DataCollectionJobDocument
{
	[BsonId]
	public ObjectId Id { get; init; } = ObjectId.GenerateNewId();
	public ObjectId UserId { get; init; }
	public required string Symbol { get; init; }

	[BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
	public required DateTime FromUtc { get; init; }

	[BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
	public required DateTime ToUtc { get; init; }
	public required TimeSpan Interval { get; init; }
	public string? CollectionName { get; init; }
	public CollectionState State { get; set; } = CollectionState.Pending;
	public int TotalChunks { get; set; }
	public int CompletedChunks { get; set; }

	[BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
	public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

	[BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
	public DateTime? StartedAt { get; set; }

	[BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
	public DateTime? CompletedAt { get; set; }
	public string? ErrorMessage { get; set; }
	public List<DataChunkDocument> Chunks { get; init; } = [];
}