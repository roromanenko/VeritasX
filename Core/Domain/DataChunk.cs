namespace Core.Domain;

public sealed class DataChunk
{
	public required DateTimeOffset FromUtc { get; init; }
	public required DateTimeOffset ToUtc { get; init; }

	public ChunkState State { get; set; }
	public int RetryCount { get; set; }
	public string? ErrorMessage { get; set; }
}