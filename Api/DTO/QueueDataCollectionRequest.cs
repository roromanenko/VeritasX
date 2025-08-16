namespace VeritasX.Core.DTO;

public class QueueDataCollectionRequest
{
	public string Symbol { get; set; } = string.Empty;
	public DateTime? FromUtc { get; set; }
	public DateTime? ToUtc { get; set; }
	public int IntervalMinutes { get; set; }
}