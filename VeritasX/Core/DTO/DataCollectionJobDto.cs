using VeritasX.Core.Domain;

namespace VeritasX.Core.DTO;

public record DataCollectionJobDto(
	string Id,
	string Symbol,
	DateTime FromUtc,
	DateTime ToUtc,
	TimeSpan Interval,
	CollectionState State,
	int TotalChunks,
	int CompletedChunks,
	DateTime CreatedAt,
	DateTime? StartedAt,
	DateTime? CompletedAt,
	string? ErrorMessage
);