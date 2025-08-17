using System.Text.Json;

namespace Trading.Strategies;

public class RebalanceToTargetStrategy : ITradingStrategy
{
	public async Task<TradingSolution> CalculateNextStep(
	string strategyConfigurationJson,
	TradingContext context,
	CancellationToken cancellationToken = default)
	{
		var cfg = RebalanceConfig.FromJson(strategyConfigurationJson);
		var asset = cfg.Asset.ToUpperInvariant();
		var baseline = context.Account.Baseline;

		var balances = context.Account.GetBalances();
		balances.TryGetValue(asset, out var assetQty);
		balances.TryGetValue(baseline, out var baselineQty);

		var price = await context.GetPriceInBaselineAsync(asset, cancellationToken).ConfigureAwait(false);
		var total = await context.GetTotalInBaseline(cancellationToken).ConfigureAwait(false);

		// Nothing to do if portfolio is empty or price unavailable
		if (total <= 0m || price <= 0m)
			return Noop(asset);

		var currentValue = assetQty * price;
		var currentWeight = currentValue / total;

		var lower = cfg.TargetWeight - cfg.Threshold;
		var upper = cfg.TargetWeight + cfg.Threshold;

		// Inside band → do nothing
		if (currentWeight >= lower && currentWeight <= upper)
			return Noop(asset);

		// Rebalance back to target exactly
		var desiredValue = cfg.TargetWeight * total;
		var deltaValue = desiredValue - currentValue; // >0 buy, <0 sell

		if (deltaValue > 0m)
		{
			// BUY using baseline
			var qtyToBuy = deltaValue / price;

			// Cap by available baseline (can't spend more than you have)
			var maxBuyQty = baselineQty / price;
			qtyToBuy = Math.Min(qtyToBuy, maxBuyQty);
			if (!PassesGuards(qtyToBuy, price, cfg.MinQty, cfg.MinNotional))
				return Noop(asset);

			return new TradingSolution { Asset = asset, Quantity = qtyToBuy, Type = SolutionType.Buy };
		}
		else
		{
			// SELL down to target
			var qtyToSell = Math.Abs(deltaValue) / price;

			// Cap by current holdings
			qtyToSell = Math.Min(qtyToSell, assetQty);
			if (!PassesGuards(qtyToSell, price, cfg.MinQty, cfg.MinNotional))
				return Noop(asset);

			return new TradingSolution { Asset = asset, Quantity = qtyToSell, Type = SolutionType.Sell };
		}
	}

	private static TradingSolution Noop(string asset) =>
		new TradingSolution { Asset = asset, Quantity = 0m, Type = SolutionType.Hold };

	private static bool PassesGuards(decimal qty, decimal price, decimal? minQty, decimal? minNotional)
	{
		if (qty <= 0m) return false;
		if (minQty.HasValue && qty < minQty.Value) return false;
		var notional = qty * price;
		if (minNotional.HasValue && notional < minNotional.Value) return false;
		return true;
	}
}

public sealed class RebalanceConfig
{
	/// Asset to keep at target weight (e.g., "BTC")
	public required string Asset { get; init; }

	/// Target weight — accepts 0..1 (0.5) or percent (50)
	public decimal TargetWeight { get; init; } = 0.5m;

	/// Rebalance threshold band — accepts 0..1 (0.1) or percent (10)
	public decimal Threshold { get; init; } = 0.1m;

	/// Exchange constraints (optional)
	public decimal? MinQty { get; init; }       // e.g., 0.0001m BTC
	public decimal? MinNotional { get; init; }  // e.g., 5 USDT

	public static RebalanceConfig FromJson(string json)
	{
		var opt = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
		var cfg = JsonSerializer.Deserialize<RebalanceConfig>(json, opt)
				  ?? throw new ArgumentException("Invalid strategyConfigurationJson");
		return cfg;
	}
}