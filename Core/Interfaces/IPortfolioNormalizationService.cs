namespace Core.Interfaces;

public interface IPortfolioNormalizationService
{
	/// <summary>
	/// Converts amount in quoteAsset to USD equivalent.
	/// Returns the original amount if conversion is unavailable.
	/// </summary>
	Task<decimal> ToUsdAsync(
		decimal amount,
		string quoteAsset,
		string exchange,
		CancellationToken ct = default);

	/// <summary>
	/// Converts amount in asset to USD using a pre-fetched price dictionary.
	/// Returns 0 if the asset price cannot be determined.
	/// </summary>
	decimal ToUsd(decimal amount, string asset, IReadOnlyDictionary<string, decimal> allPrices);
}
