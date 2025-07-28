using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VeritasX.Core.Interfaces;
using VeritasX.Core.Options;

namespace VeritasX.Infrastructure.Jobs;

public class DatabaseCleanupJob : BackgroundService
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<DatabaseCleanupJob> _logger;
	private readonly DatabaseCleanupOptions _options;

	public DatabaseCleanupJob(
		IServiceProvider serviceProvider, 
		ILogger<DatabaseCleanupJob> logger,
		IOptions<DatabaseCleanupOptions> options)
	{
		_serviceProvider = serviceProvider;
		_logger = logger;
		_options = options.Value;
	}

	protected override async Task ExecuteAsync(CancellationToken cancellationToken)
	{
		if (!_options.EnabledCleanupService)
		{
			_logger.LogInformation("Database cleanup service is disabled");
			return;
		}

		_logger.LogInformation("Database cleanup service started. Cleanup interval: {Interval}, Max age: {MaxAge}", 
			_options.CleanupInterval, _options.MaxRecordAge);

		while (!cancellationToken.IsCancellationRequested)
		{
			try
			{
				await CleanupDatabaseAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error during database cleanup");
			}

			await Task.Delay(_options.CleanupInterval, cancellationToken);
		}
	}

	private async Task CleanupDatabaseAsync(CancellationToken cancellationToken)
	{
		using var scope = _serviceProvider.CreateScope();
		var cleanupRepository = scope.ServiceProvider.GetRequiredService<IDatabaseCleanupRepository>();

		var cutoffDate = DateTime.UtcNow - _options.MaxRecordAge;

		long totalDeletedChunks = 0;
		long totalDeletedJobs = 0;
		int batchNum = 0;

		var totalCount = await cleanupRepository.GetCompletedJobCountAsync(cutoffDate, cancellationToken);
		_logger.LogInformation("Found {TotalCount} completed jobs older than {CutoffDate}", totalCount, cutoffDate);

		if (totalCount == 0)
		{
			return;
		}

		await foreach (var batch in cleanupRepository.GetJobIdBatchesAsync(cutoffDate, _options.JobBatchSize, cancellationToken))
		{
			batchNum++;
			_logger.LogInformation("Processing batch {BatchNum} with {BatchSize} jobs", batchNum, batch.Count());

			totalDeletedChunks += await cleanupRepository.CleanupCandleChunksByJobIdsAsync(batch, cancellationToken);
			totalDeletedJobs += await cleanupRepository.CleanupDataCollectionJobsByIdsAsync(batch, cancellationToken);
		}

		_logger.LogInformation("Database cleanup completed. Deleted {JobCount} jobs and {ChunkCount} chunks", 
			totalDeletedJobs, totalDeletedChunks);
	}
}