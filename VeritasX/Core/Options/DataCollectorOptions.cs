namespace VeritasX.Core.Options;

public class DataCollectorOptions
{
	public int MaxCandlesPerRequest { get; set; } = 500;
	public int RequestsPerMinute { get; set; } = 1000;
	public int RetryAttempts { get; set; } = 3;
	public int BatchSize { get; set; } = 500;
	public int DelayBetweenRequestsMs { get; set; } = 100;
	public int MaxConcurrentJobs { get; set; } = 3;
}