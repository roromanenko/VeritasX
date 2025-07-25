namespace VeritasX.Core.Options;

public class DatabaseCleanupOptions
{
	public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromDays(1);
	public TimeSpan MaxRecordAge { get; set; } = TimeSpan.FromDays(1);
	public bool EnabledCleanupService { get; set; } = true;
	public int JobBatchSize { get; set; } = 100;
}