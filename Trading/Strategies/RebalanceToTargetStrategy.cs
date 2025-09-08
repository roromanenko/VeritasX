using System.Text.Json;

namespace Trading.Strategies;

public class RebalanceToTargetStrategy : ITradingStrategy
{
	private readonly RebalanceConfig _config;

	public RebalanceToTargetStrategy(RebalanceConfig config)
	{
		_config = config;
	}

	public async Task<TradingSolution> CalculateNextStep(TradingContext context, CancellationToken cancellationToken = default)
	{
		var asset = _config.Asset.ToUpperInvariant();
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

		var lower = _config.TargetWeight - _config.Threshold;
		var upper = _config.TargetWeight + _config.Threshold;

		// Inside band → do nothing
		if (currentWeight >= lower && currentWeight <= upper)
			return Noop(asset);

		// Rebalance back to target exactly
		var desiredValue = _config.TargetWeight * total;
		var deltaValue = desiredValue - currentValue; // >0 buy, <0 sell

		if (deltaValue > 0m)
		{
			// BUY using baseline
			var qtyToBuy = deltaValue / price;

			// Cap by available baseline (can't spend more than you have)
			var maxBuyQty = baselineQty / price;
			qtyToBuy = Math.Min(qtyToBuy, maxBuyQty);
			if (!PassesGuards(qtyToBuy, price, _config.MinQty, _config.MinNotional))
				return Noop(asset);

			return Buy(asset, qtyToBuy);
		}
		else
		{
			// SELL down to target
			var qtyToSell = Math.Abs(deltaValue) / price;

			// Cap by current holdings
			qtyToSell = Math.Min(qtyToSell, assetQty);
			if (!PassesGuards(qtyToSell, price, _config.MinQty, _config.MinNotional))
				return Noop(asset);

			return Sell(asset, qtyToSell);
		}
	}

	private static TradingSolution Noop(string asset) =>
		new TradingSolution { Asset = asset, Quantity = 0m, Type = SolutionType.Hold };
	private static TradingSolution Buy(string asset, decimal quantity) =>
		new TradingSolution { Asset = asset, Quantity = quantity, Type = SolutionType.Buy };
	private static TradingSolution Sell(string asset, decimal quantity) =>
		new TradingSolution { Asset = asset, Quantity = quantity, Type = SolutionType.Sell };

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