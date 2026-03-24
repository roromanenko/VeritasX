using Core.Domain;
using Core.Domain.Statistics;
using Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class PortfolioSnapshotService : IPortfolioSnapshotService
{
	private readonly IUserService _userService;
	private readonly IExchangeServiceFactory _exchangeServiceFactory;
	private readonly IPortfolioNormalizationService _normalization;
	private readonly IPriceProvider _priceProvider;
	private readonly IStatisticsCache _cache;
	private readonly ILogger<PortfolioSnapshotService> _logger;

	public PortfolioSnapshotService(
		IUserService userService,
		IExchangeServiceFactory exchangeServiceFactory,
		IPortfolioNormalizationService normalization,
		IPriceProvider priceProvider,
		IStatisticsCache cache,
		ILogger<PortfolioSnapshotService> logger)
	{
		_userService = userService;
		_exchangeServiceFactory = exchangeServiceFactory;
		_normalization = normalization;
		_priceProvider = priceProvider;
		_cache = cache;
		_logger = logger;
	}

	public async Task<List<ExchangeSummary>> GetPortfolioSnapshotAsync(
		string userId,
		CancellationToken ct = default)
	{
		var connections = await _userService.GetAllExchangeConnections(userId);
		var summaries = new List<ExchangeSummary>();

		foreach (var (exchangeName, connection) in connections)
		{
			try
			{
				var allPrices = await GetAllPricesCachedAsync(exchangeName.ToString(), ct);

				var exchangeService = _exchangeServiceFactory.Create(exchangeName);
				var portfolio = await exchangeService.GetPortfolio(userId, connection, ct);

				var assets = new List<Balance>();
				foreach (var balance in portfolio.Balances.Where(b => b.Total > 0))
				{
					var usdValue = _normalization.ToUsd(balance.Total, balance.Asset, allPrices);
					if (usdValue == 0)
					{
						_logger.LogDebug(
							"Asset {Asset} on {Exchange} has Total > 0 but USD value is 0 — skipping from sum",
							balance.Asset, exchangeName);
					}
					balance.UsdValue = usdValue;
					assets.Add(balance);
				}

				var equityUsd = assets.Sum(b => b.UsdValue);
				summaries.Add(new ExchangeSummary
				{
					Exchange = exchangeName.ToString(),
					EquityUsd = equityUsd,
					AllocationPercent = 0,
					BotCount = 0,
					ActiveBotCount = 0,
					Assets = assets.OrderByDescending(b => b.UsdValue).ToList()
				});
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex,
					"Failed to retrieve portfolio for exchange {Exchange} — skipping",
					exchangeName);
			}
		}

		var totalUsd = summaries.Sum(s => s.EquityUsd);
		var result = summaries
			.Select(s => new ExchangeSummary
			{
				Exchange = s.Exchange,
				EquityUsd = s.EquityUsd,
				AllocationPercent = totalUsd > 0 ? s.EquityUsd / totalUsd * 100 : 0,
				BotCount = 0,
				ActiveBotCount = 0,
				Assets = s.Assets
			})
			.OrderByDescending(s => s.EquityUsd)
			.ToList();

		return result;
	}

	private async Task<IReadOnlyDictionary<string, decimal>> GetAllPricesCachedAsync(
		string exchange,
		CancellationToken ct)
	{
		var cacheKey = $"stats:prices:{exchange}";
		var cached = await _cache.GetAsync<Dictionary<string, decimal>>(cacheKey, ct);
		if (cached != null)
			return cached;

		var prices = await _priceProvider.GetAllPricesAsync(exchange, ct);
		await _cache.SetAsync(cacheKey, prices, TimeSpan.FromSeconds(30), ct);
		return prices;
	}
}
