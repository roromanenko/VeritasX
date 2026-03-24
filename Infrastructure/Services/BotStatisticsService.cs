using Core.Domain.Statistics;
using Core.Interfaces;
using Infrastructure.Interfaces;
using Infrastructure.Persistence.Entities;
using MongoDB.Bson;

namespace Infrastructure.Services;

public class BotStatisticsService : IBotStatisticsService
{
	private readonly IBotStatisticsRepository _statisticsRepository;
	private readonly IBotRepository _botRepository;
	private readonly IStatisticsCache _cache;
	private readonly IPortfolioNormalizationService _normalization;

	public BotStatisticsService(
		IBotStatisticsRepository statisticsRepository,
		IBotRepository botRepository,
		IStatisticsCache cache,
		IPortfolioNormalizationService normalization)
	{
		_statisticsRepository = statisticsRepository;
		_botRepository = botRepository;
		_cache = cache;
		_normalization = normalization;
	}

	public async Task<BotStatistics?> GetBotStatisticsAsync(string botId, string userId, DateOnly? from, DateOnly? to, CancellationToken ct = default)
	{
		var cacheKey = $"stats:botstats:{botId}";
		var cached = await _cache.GetAsync<BotStatistics>(cacheKey, ct);
		if (cached is not null)
			return cached;

		if (!ObjectId.TryParse(botId, out var botObjectId) || !ObjectId.TryParse(userId, out var userObjectId))
			return null;

		var bot = await _botRepository.GetBotById(botObjectId, userObjectId);
		if (bot is null)
			return null;

		var snapshots = await _statisticsRepository.GetSnapshotsAsync(botObjectId, from, to);

		var currentEquity = await _cache.GetAsync<decimal?>($"stats:equity:{botId}", ct)
			?? (snapshots.Count > 0 ? snapshots[^1].ClosingEquity : 0m);

		var result = ComputeStatistics(botId, bot.Symbol, currentEquity, snapshots);

		await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(2), ct);
		return result;
	}

	public async Task<AccountStatistics> GetAccountStatisticsAsync(string userId, DateOnly? from, DateOnly? to, CancellationToken ct = default)
	{
		var cacheKey = $"stats:accountstats:{userId}";
		var cached = await _cache.GetAsync<AccountStatistics>(cacheKey, ct);
		if (cached is not null)
			return cached;

		if (!ObjectId.TryParse(userId, out var userObjectId))
			return new AccountStatistics();

		var latestSnapshots = await _statisticsRepository.GetLatestSnapshotPerBotAsync(userObjectId);

		var botDocs = await _botRepository.GetBotsByUserId(userObjectId);
		var botDocMap = botDocs.ToDictionary(b => b.Id);

		var summaries = new List<BotStatisticsSummary>();
		var totalPnl = 0m;
		var totalEquityUsd = 0m;

		// exchangeName → (equityUsd, list of botIds)
		var exchangeGroups = new Dictionary<string, (decimal EquityUsd, List<ObjectId> BotIds)>(StringComparer.OrdinalIgnoreCase);

		foreach (var snapshot in latestSnapshots)
		{
			var currentEquity = await _cache.GetAsync<decimal?>($"stats:equity:{snapshot.BotId}", ct)
				?? snapshot.ClosingEquity;

			var equityUsd = await _normalization.ToUsdAsync(currentEquity, snapshot.QuoteAsset, snapshot.Exchange, ct);

			totalEquityUsd += equityUsd;
			totalPnl += snapshot.RealizedPnl;

			var totalRoundTrips = snapshot.WinCount + snapshot.LossCount;
			var winRate = totalRoundTrips > 0 ? (decimal)snapshot.WinCount / totalRoundTrips : 0m;

			summaries.Add(new BotStatisticsSummary
			{
				BotId = snapshot.BotId.ToString(),
				Symbol = botDocMap.TryGetValue(snapshot.BotId, out var doc) ? doc.Symbol : string.Empty,
				Exchange = snapshot.Exchange,
				QuoteAsset = snapshot.QuoteAsset,
				CurrentEquity = currentEquity,
				CurrentEquityUsd = equityUsd,
				RealizedPnl = snapshot.RealizedPnl,
				WinRate = winRate,
				TradeCount = snapshot.TradeCount
			});

			var exchangeKey = string.IsNullOrEmpty(snapshot.Exchange) ? "Unknown" : snapshot.Exchange;
			if (!exchangeGroups.TryGetValue(exchangeKey, out var group))
				group = (0m, []);
			exchangeGroups[exchangeKey] = (group.EquityUsd + equityUsd, [.. group.BotIds, snapshot.BotId]);
		}

		var byExchange = exchangeGroups.Select(kvp =>
		{
			var activeBotCount = kvp.Value.BotIds.Count(id =>
				botDocMap.TryGetValue(id, out var d) && d.Status == Core.Domain.BotStatus.Active);
			var allocation = totalEquityUsd > 0 ? kvp.Value.EquityUsd / totalEquityUsd * 100 : 0m;
			return new ExchangeSummary
			{
				Exchange = kvp.Key,
				EquityUsd = kvp.Value.EquityUsd,
				AllocationPercent = allocation,
				BotCount = kvp.Value.BotIds.Count,
				ActiveBotCount = activeBotCount
			};
		}).ToList();

		var result = new AccountStatistics
		{
			TotalEquityUsd = totalEquityUsd,
			TotalRealizedPnl = totalPnl,
			ByExchange = byExchange,
			Bots = summaries
		};

		await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(2), ct);
		return result;
	}

	private static BotStatistics ComputeStatistics(string botId, string symbol, decimal currentEquity, List<BotDailyStatisticsDocument> snapshots)
	{
		if (snapshots.Count == 0)
		{
			return new BotStatistics
			{
				BotId = botId,
				Symbol = symbol,
				CurrentEquity = currentEquity
			};
		}

		var closingEquities = snapshots.Select(s => s.ClosingEquity).ToArray();
		var dailyReturns = ComputeDailyReturns(closingEquities);

		var volatility = 0m;
		decimal? sharpe = null;
		decimal? sortino = null;
		decimal? calmar = null;

		if (dailyReturns.Length > 0)
		{
			var stdDev = StdDev(dailyReturns);
			volatility = stdDev * (decimal)Math.Sqrt(365);

			if (dailyReturns.Length >= 30 && volatility > 0)
				sharpe = Mean(dailyReturns) * 365 / volatility;

			var negativeReturns = dailyReturns.Where(r => r < 0).ToArray();
			if (negativeReturns.Length > 0)
			{
				var downsideDeviation = StdDev(negativeReturns) * (decimal)Math.Sqrt(365);
				if (dailyReturns.Length >= 30 && downsideDeviation > 0)
					sortino = Mean(dailyReturns) * 365 / downsideDeviation;
			}
		}

		var maxDrawdownPercent = ComputeMaxDrawdown(closingEquities);
		var annualizedReturn = dailyReturns.Length > 0 ? Mean(dailyReturns) * 365 : 0m;
		if (maxDrawdownPercent > 0)
			calmar = annualizedReturn / maxDrawdownPercent;

		var totalRoundTrips = snapshots.Sum(s => s.WinCount + s.LossCount);
		var totalWins = snapshots.Sum(s => s.WinCount);
		var winRate = totalRoundTrips > 0 ? (decimal)totalWins / totalRoundTrips : 0m;

		var grossProfit = snapshots.Sum(s => s.GrossProfit);
		var grossLoss = snapshots.Sum(s => s.GrossLoss);
		decimal? profitFactor = grossLoss > 0 ? grossProfit / grossLoss : null;

		var equityCurve = BuildEquityCurve(snapshots, closingEquities, dailyReturns);

		return new BotStatistics
		{
			BotId = botId,
			Symbol = symbol,
			CurrentEquity = currentEquity,
			Sharpe = sharpe,
			Sortino = sortino,
			Calmar = calmar,
			MaxDrawdownPercent = maxDrawdownPercent,
			Volatility = volatility,
			WinRate = winRate,
			ProfitFactor = profitFactor,
			TradeCount = snapshots.Sum(s => s.TradeCount),
			TotalRoundTrips = totalRoundTrips,
			TotalRealizedPnl = snapshots.Sum(s => s.RealizedPnl),
			TotalFees = snapshots.Sum(s => s.Fees),
			EquityCurve = equityCurve
		};
	}

	private static List<EquityPoint> BuildEquityCurve(List<BotDailyStatisticsDocument> snapshots, decimal[] closingEquities, decimal[] dailyReturns)
	{
		var curve = new List<EquityPoint>(snapshots.Count);
		for (var i = 0; i < snapshots.Count; i++)
		{
			curve.Add(new EquityPoint
			{
				Date = snapshots[i].Date,
				Equity = closingEquities[i],
				DailyReturn = i < dailyReturns.Length ? dailyReturns[i] : 0m
			});
		}
		return curve;
	}

	private static decimal[] ComputeDailyReturns(decimal[] equities)
	{
		if (equities.Length < 2)
			return Array.Empty<decimal>();

		var returns = new decimal[equities.Length - 1];
		for (var i = 1; i < equities.Length; i++)
		{
			returns[i - 1] = equities[i - 1] > 0
				? (equities[i] - equities[i - 1]) / equities[i - 1]
				: 0m;
		}
		return returns;
	}

	private static decimal ComputeMaxDrawdown(decimal[] equities)
	{
		if (equities.Length == 0)
			return 0m;

		var peak = equities[0];
		var maxDrawdown = 0m;

		foreach (var equity in equities)
		{
			if (equity > peak)
				peak = equity;

			var drawdown = peak > 0 ? (peak - equity) / peak : 0m;
			if (drawdown > maxDrawdown)
				maxDrawdown = drawdown;
		}

		return maxDrawdown;
	}

	private static decimal Mean(decimal[] values)
	{
		if (values.Length == 0)
			return 0m;
		return values.Sum() / values.Length;
	}

	private static decimal StdDev(decimal[] values)
	{
		if (values.Length == 0)
			return 0m;

		var mean = Mean(values);
		var sumSquares = values.Sum(v => (v - mean) * (v - mean));
		return (decimal)Math.Sqrt((double)(sumSquares / values.Length));
	}
}
