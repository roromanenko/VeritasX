namespace Core.Domain;

public sealed class DataCollectionJob
{
	public required string Id { get; init; }
	public required string UserId { get; init; }
	public required string Symbol { get; init; }
	public required DateTimeOffset FromUtc { get; init; }
	public required DateTimeOffset ToUtc { get; init; }
	public required TimeSpan Interval { get; init; }

	public string? CollectionName { get; set; }
	public CollectionState State { get; set; }

	public int TotalChunks { get; set; }
	public int CompletedChunks { get; set; }

	public DateTimeOffset CreatedAt { get; init; }
	public DateTimeOffset? StartedAt { get; set; }
	public DateTimeOffset? CompletedAt { get; set; }
	public string? ErrorMessage { get; set; }

	public List<DataChunk> Chunks { get; set; } = [];
}