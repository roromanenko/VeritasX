namespace Api.DTO;

public record QueueDataCollectionRequest
(
	string Symbol,
	DateTime? FromUtc,
	DateTime? ToUtc,
	int IntervalMinutes
);