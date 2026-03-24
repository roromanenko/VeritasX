using Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class PortfolioNormalizationService : IPortfolioNormalizationService
{
	private static readonly HashSet<string> _stablecoins =
		new(StringComparer.OrdinalIgnoreCase) { "USDT", "USDC", "BUSD", "DAI", "USD" };

	private static readonly HashSet<string> _fiatCurrencies =
		new(StringComparer.OrdinalIgnoreCase)
		{
			"TRY", "UAH", "RUB", "JPY", "EUR", "GBP", "BRL",
			"ARS", "PLN", "MXN", "COP", "CZK", "ZAR", "IDR"
		};

	private readonly IPriceProvider _priceProvider;
	private readonly ILogger<PortfolioNormalizationService> _logger;

	public PortfolioNormalizationService(IPriceProvider priceProvider,
		ILogger<PortfolioNormalizationService> logger)
	{
		_priceProvider = priceProvider;
		_logger = logger;
	}

	public async Task<decimal> ToUsdAsync(
		decimal amount,
		string quoteAsset,
		string exchange,
		CancellationToken ct = default)
	{
		if (_stablecoins.Contains(quoteAsset))
			return amount;

		try
		{
			var price = await _priceProvider.GetPriceAsync(quoteAsset, "USDT", ct);
			return amount * price;
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex,
				"Could not get price for {QuoteAsset}USDT on {Exchange}. Returning original amount.",
				quoteAsset, exchange);
			return amount;
		}
	}

	public decimal ToUsd(decimal amount, string asset, IReadOnlyDictionary<string, decimal> allPrices)
	{
		if (_stablecoins.Contains(asset))
			return amount;

		var symbol = $"{asset}USDT";
		if (allPrices.TryGetValue(symbol, out var price))
			return amount * price;

		if (_fiatCurrencies.Contains(asset))
			_logger.LogDebug(
				"Fiat currency {Asset} has no USDT pair in price feed — excluding from portfolio value",
				asset);
		else
			_logger.LogDebug(
				"Asset {Asset} has no USDT pair in price feed — excluding from portfolio value",
				asset);

		return 0m;
	}
}
