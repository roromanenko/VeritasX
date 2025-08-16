using Core.Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Infrastructure.Persistence.Entities;

public record CandleChunkDocument
{
	[BsonId]
	public ObjectId Id { get; init; } = ObjectId.GenerateNewId();
	public ObjectId JobId { get; init; }
	public required string Symbol { get; init; }
	public required DateTimeOffset FromUtc { get; init; }
	public required DateTimeOffset ToUtc { get; init; }
	public required TimeSpan Interval { get; init; }
	public required List<CandleDocument> Candles { get; init; } = [];
	public DateTimeOffset CreatedAt { get; init; } = DateTime.UtcNow;
	public int CandleCount => Candles.Count;
}
