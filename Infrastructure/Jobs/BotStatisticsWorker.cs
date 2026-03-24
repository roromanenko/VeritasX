using Core.Domain;
using Core.Domain.Statistics;
using Core.Interfaces;
using Infrastructure.Interfaces;
using Infrastructure.Persistence.Entities;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Jobs;

public class BotStatisticsWorker : BackgroundService
{
	private static readonly TimeSpan EquityUpsertInterval = TimeSpan.FromMinutes(5);
	private static readonly TimeSpan EquityCacheTtl = TimeSpan.FromMinutes(2);
	private static readonly TimeSpan LastUpsertCacheTtl = TimeSpan.FromMinutes(6);

	private readonly BotStatisticsUpdater _updater;
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly IStatisticsCache _cache;
	private readonly ILogger<BotStatisticsWorker> _logger;

	public BotStatisticsWorker(
		BotStatisticsUpdater updater,
		IServiceScopeFactory scopeFactory,
		IStatisticsCache cache,
		ILogger<BotStatisticsWorker> logger)
	{
		_updater = updater;
		_scopeFactory = scopeFactory;
		_cache = cache;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var tickTask = ProcessTickChannelAsync(stoppingToken);
		var tradeTask = ProcessTradeChannelAsync(stoppingToken);
		await Task.WhenAll(tickTask, tradeTask);
	}

	private async Task ProcessTickChannelAsync(CancellationToken ct)
	{
		await foreach (var evt in _updater.TickReader.ReadAllAsync(ct))
		{
			try
			{
				var equityKey = $"stats:equity:{evt.BotId}";
				await _cache.SetAsync(equityKey, evt.Equity, EquityCacheTtl, ct);

				var lastUpsertKey = $"stats:lastupsert:{evt.BotId}";
				var lastUpsert = await _cache.GetAsync<DateTimeOffset?>(lastUpsertKey, ct);
				var now = DateTimeOffset.UtcNow;

				if (lastUpsert is null || (now - lastUpsert.Value) > EquityUpsertInterval)
				{
					using var scope = _scopeFactory.CreateScope();
					var repo = scope.ServiceProvider.GetRequiredService<IBotStatisticsRepository>();
					var today = DateOnly.FromDateTime(now.UtcDateTime);

					await repo.UpsertTickEquityAsync(
						ObjectId.Parse(evt.BotId),
						ObjectId.Parse(evt.UserId),
						today,
						evt.Equity,
						evt.Price,
						now);

					await _cache.SetAsync(lastUpsertKey, (DateTimeOffset?)now, LastUpsertCacheTtl, ct);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing tick event for bot {BotId}", evt.BotId);
			}
		}
	}

	private async Task ProcessTradeChannelAsync(CancellationToken ct)
	{
		await foreach (var evt in _updater.TradeReader.ReadAllAsync(ct))
		{
			try
			{
				using var scope = _scopeFactory.CreateScope();
				var repo = scope.ServiceProvider.GetRequiredService<IBotStatisticsRepository>();

				try
				{
					await repo.InsertProcessedTradeAsync(new StatisticsProcessedTradeDocument
					{
						TradeId = evt.TradeId,
						ProcessedAt = DateTimeOffset.UtcNow
					});
				}
				catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
				{
					_logger.LogDebug("Trade {TradeId} already processed, skipping", evt.TradeId);
					continue;
				}

				OpenPositionEntry? matched = null;
				var today = DateOnly.FromDateTime(evt.Timestamp.UtcDateTime);
				var botId = ObjectId.Parse(evt.BotId);
				var userId = ObjectId.Parse(evt.UserId);

				if (evt.Side == OrderSide.Buy)
				{
					await repo.PushOpenPositionAsync(botId, evt.Symbol, new OpenPositionEntry
					{
						TradeId = evt.TradeId,
						Price = evt.Price,
						Quantity = evt.Quantity,
						ExecutedAt = evt.Timestamp
					});
				}
				else
				{
					matched = await repo.PopOpenPositionAsync(botId, evt.Symbol);
				}

				var roundTripPnl = matched is not null
					? (evt.Price - matched.Price) * evt.Quantity
					: 0m;
				bool? isWin = matched is not null ? roundTripPnl > 0 : null;

				var realizedPnl = evt.Side == OrderSide.Sell
					? evt.QuoteQuantity - (matched?.Price ?? 0) * evt.Quantity
					: 0m;

				await repo.IncrementTradeAsync(
					botId, userId, today,
					evt.EquityAfterTrade, evt.Price,
					realizedPnl, evt.Fee,
					isWin, roundTripPnl);

				await _cache.InvalidateAsync($"stats:botstats:{evt.BotId}", ct);
				await _cache.InvalidateAsync($"stats:accountstats:{evt.UserId}", ct);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing trade event for bot {BotId}", evt.BotId);
			}
		}
	}
}
