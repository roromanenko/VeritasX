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
	public required DateTimeOffset FromUtc { get; init; }
	public required DateTimeOffset ToUtc { get; init; }
	public required TimeSpan Interval { get; init; }
	public string? CollectionName { get; init; }
	public CollectionState State { get; set; } = CollectionState.Pending;
	public int TotalChunks { get; set; }
	public int CompletedChunks { get; set; }
	public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
	public DateTimeOffset? StartedAt { get; set; }
	public DateTimeOffset? CompletedAt { get; set; }
	public string? ErrorMessage { get; set; }
	public List<DataChunkDocument> Chunks { get; init; } = [];
}