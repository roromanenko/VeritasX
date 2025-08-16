using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using Core.Domain;
using Core.Options;
using Core.Interfaces;
using Infrastructure.Persistence.Entities;
using MongoDB.Bson;

namespace Infrastructure.Jobs;

public class DataCollectorBackgroundService : BackgroundService
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<DataCollectorBackgroundService> _logger;
	private readonly DataCollectorOptions _options;
	private readonly SemaphoreSlim _rateLimitSemaphore;
	private readonly ConcurrentDictionary<string, DataCollectionJob> _activeJobs = new();

	public DataCollectorBackgroundService(
		IServiceProvider serviceProvider,
		ILogger<DataCollectorBackgroundService> logger,
		IOptions<DataCollectorOptions> options)
	{
		_serviceProvider = serviceProvider;
		_logger = logger;
		_options = options.Value;
		_rateLimitSemaphore = new SemaphoreSlim(_options.RequestsPerMinute, _options.RequestsPerMinute);

		// Восстанавливаем семафор каждую минуту
		_ = Task.Run(async () =>
		{
			while (true)
			{
				await Task.Delay(TimeSpan.FromMinutes(1));
				var currentCount = _rateLimitSemaphore.CurrentCount;
				if (currentCount < _options.RequestsPerMinute)
				{
					_rateLimitSemaphore.Release(_options.RequestsPerMinute - currentCount);
				}
			}
		});
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("DataCollectorBackgroundService started");

		var tasks = new List<Task>();

		while (!stoppingToken.IsCancellationRequested)
		{
			// Проверяем, можем ли взять новое задание
			if (_activeJobs.Count < _options.MaxConcurrentJobs)
			{
				using var scope = _serviceProvider.CreateScope();
				var dataService = scope.ServiceProvider.GetRequiredService<IDataCollectionService>();

				var job = await dataService.GetNextPendingJobAsync();
				if (job != null)
				{
					_activeJobs[job.Id] = job;
					tasks.Add(ProcessJobAsync(job, stoppingToken));
				}
			}

			// Удаляем завершенные задачи
			tasks.RemoveAll(t => t.IsCompleted);

			await Task.Delay(1000, stoppingToken);
		}

		await Task.WhenAll(tasks);
	}

	private async Task ProcessJobAsync(DataCollectionJob job, CancellationToken cancellationToken)
	{
		try
		{
			job.State = CollectionState.InProgress;
			job.StartedAt = DateTime.UtcNow;

			using var scope = _serviceProvider.CreateScope();
			var dataService = scope.ServiceProvider.GetRequiredService<IDataCollectionService>();
			var chunkService = scope.ServiceProvider.GetRequiredService<ICandleChunkService>();
			await dataService.UpdateJobAsync(job);

			_logger.LogInformation("Started processing job {JobId} for {Symbol}", job.Id, job.Symbol);

			var priceProvider = scope.ServiceProvider.GetRequiredService<IPriceProvider>();
			int totalCandles = 0;

			foreach (var chunk in job.Chunks.Where(c => c.State != ChunkState.Completed))
			{
				if (cancellationToken.IsCancellationRequested || job.State == CollectionState.Cancelled)
					break;

				if (await chunkService.ChunkExistsAsync(job.Id.ToString(), chunk.FromUtc.UtcDateTime, chunk.ToUtc.UtcDateTime))
				{
					chunk.State = ChunkState.Completed;
					job.CompletedChunks++;
					continue;
				}

				var candles = await ProcessChunkAsync(chunk, job, priceProvider, dataService, cancellationToken);

				if (candles.Any())
				{
					// Создаем и сохраняем чанк с данными
					var candleChunk = new CandleChunk
					{
						JobId = job.Id,
						Symbol = job.Symbol,
						FromUtc = chunk.FromUtc,
						ToUtc = chunk.ToUtc,
						Interval = job.Interval,
						Candles = candles.ToList()
					};

					await chunkService.SaveChunkAsync(candleChunk);
					totalCandles += candles.Count();

					_logger.LogDebug("Saved chunk for job {JobId}: {From} - {To}, {CandleCount} candles",
						job.Id, chunk.FromUtc, chunk.ToUtc, candles.Count());
				}
			}

			job.State = CollectionState.Completed;
			job.CompletedAt = DateTime.UtcNow;

			_logger.LogInformation("Completed job {JobId} for {Symbol}. Collected {CandleCount} candles in {ChunkCount} chunks",
				job.Id, job.Symbol, totalCandles, job.CompletedChunks);
		}
		catch (Exception ex)
		{
			job.State = CollectionState.Failed;
			job.ErrorMessage = ex.Message;
			_logger.LogError(ex, "Failed to process job {JobId} for {Symbol}", job.Id, job.Symbol);
		}
		finally
		{
			using var scope = _serviceProvider.CreateScope();
			var dataService = scope.ServiceProvider.GetRequiredService<IDataCollectionService>();
			await dataService.UpdateJobAsync(job);

			_activeJobs.TryRemove(job.Id, out _);
		}
	}

	private async Task<IEnumerable<Candle>> ProcessChunkAsync(
		DataChunk chunk,
		DataCollectionJob job,
		IPriceProvider priceProvider,
		IDataCollectionService dataService,
		CancellationToken cancellationToken)
	{
		for (int retry = 0; retry <= _options.RetryAttempts; retry++)
		{
			try
			{
				chunk.State = ChunkState.InProgress;

				// Ждем разрешения на запрос (rate limiting)
				await _rateLimitSemaphore.WaitAsync(cancellationToken);

				try
				{
					var candles = await priceProvider.GetHistoryAsync(
						job.Symbol, chunk.FromUtc, chunk.ToUtc, job.Interval, cancellationToken);

					chunk.State = ChunkState.Completed;
					job.CompletedChunks++;

					await dataService.UpdateJobAsync(job);
					return candles;
				}
				finally
				{
					// Задержка между запросами
					if (_options.DelayBetweenRequestsMs > 0)
						await Task.Delay(_options.DelayBetweenRequestsMs, cancellationToken);
				}
			}
			catch (Exception ex) when (retry < _options.RetryAttempts)
			{
				chunk.RetryCount = retry + 1;
				chunk.ErrorMessage = ex.Message;

				var delay = TimeSpan.FromSeconds(Math.Pow(2, retry));
				await Task.Delay(delay, cancellationToken);

				_logger.LogWarning("Retry {RetryCount} for chunk {From}-{To} in job {JobId}: {Error}",
					retry + 1, chunk.FromUtc, chunk.ToUtc, job.Id, ex.Message);
			}
			catch (Exception ex)
			{
				chunk.State = ChunkState.Failed;
				chunk.ErrorMessage = ex.Message;
				_logger.LogError(ex, "Failed chunk {From}-{To} in job {JobId} after {RetryCount} retries",
					chunk.FromUtc, chunk.ToUtc, job.Id, _options.RetryAttempts);
				return [];
			}
		}

		return [];
	}
}