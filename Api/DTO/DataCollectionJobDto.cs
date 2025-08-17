using Core.Domain;

namespace Api.DTO;

public record DataCollectionJobDto
{
	public string Id { get; init; } = default!;
	public string Symbol { get; init; } = default!;
	public DateTime FromUtc { get; init; }
	public DateTime ToUtc { get; init; }
	public TimeSpan Interval { get; init; }
	public CollectionState State { get; init; }
	public int TotalChunks { get; init; }
	public int CompletedChunks { get; init; }
	public DateTime CreatedAt { get; init; }
	public DateTime? StartedAt { get; init; }
	public DateTime? CompletedAt { get; init; }
	public string? ErrorMessage { get; init; }
}