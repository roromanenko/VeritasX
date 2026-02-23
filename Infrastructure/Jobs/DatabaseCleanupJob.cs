using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Core.Interfaces;
using Core.Options;

namespace Infrastructure.Jobs;

/// <summary>
/// Background service that periodically removes outdated completed jobs and their associated data chunks from the database.<br/>
/// Runs on a configurable interval and processes records in batches to minimize database load. <br/>
/// Can be disabled via <see cref="DatabaseCleanupOptions.EnabledCleanupService"/>.
/// </summary>
public class DatabaseCleanupJob : BackgroundService
{
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly ILogger<DatabaseCleanupJob> _logger;
	private readonly DatabaseCleanupOptions _options;

	public DatabaseCleanupJob(
        IServiceScopeFactory scopeFactory,
		ILogger<DatabaseCleanupJob> logger,
		IOptions<DatabaseCleanupOptions> options)
	{
        _scopeFactory = scopeFactory;
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

    /// <summary>
    /// Performs a single cleanup cycle by deleting completed jobs older than the configured retention period
    /// and their associated candle chunks, processing records in batches.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the cleanup operation.</param>
    private async Task CleanupDatabaseAsync(CancellationToken cancellationToken)
	{
		using var scope = _scopeFactory.CreateScope();
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