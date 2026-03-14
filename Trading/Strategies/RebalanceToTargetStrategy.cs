using System.Text.Json;
using Core.Domain;

namespace Trading.Strategies;

public class RebalanceToTargetStrategy : ITradingStrategy
{
	private readonly RebalanceConfig _config;

	public RebalanceToTargetStrategy(RebalanceConfig config)
	{
		_config = config;
	}

	public Task<TradingSolution> CalculateNextStep(TradingContext context, MarketTick tick, CancellationToken ct = default)
	{
		var asset = _config.Asset.ToUpperInvariant();
		var baseline = context.Account.Baseline;

		var balances = context.Account.GetBalances();
		balances.TryGetValue(asset, out var assetQty);
		balances.TryGetValue(baseline, out var baselineQty);

		var price = tick.Price;
		var total = context.GetTotalInBaseline(price);

		if (total <= 0m || price <= 0m)
			return Task.FromResult(Noop(asset));

		var currentValue = assetQty * price;
		var currentWeight = currentValue / total;

		var lower = _config.TargetWeight - _config.Threshold;
		var upper = _config.TargetWeight + _config.Threshold;

		if (currentWeight >= lower && currentWeight <= upper)
			return Task.FromResult(Noop(asset));

		var desiredValue = _config.TargetWeight * total;
		var deltaValue = desiredValue - currentValue;

		if (deltaValue > 0m)
		{
			var qtyToBuy = deltaValue / price;
			var maxBuyQty = baselineQty / price;
			qtyToBuy = Math.Min(qtyToBuy, maxBuyQty);
			if (!PassesGuards(qtyToBuy, price, _config.MinQty, _config.MinNotional))
				return Task.FromResult(Noop(asset));
			return Task.FromResult(Buy(asset, qtyToBuy, currentWeight, _config.TargetWeight));
		}
		else
		{
			var qtyToSell = Math.Abs(deltaValue) / price;
			qtyToSell = Math.Min(qtyToSell, assetQty);
			if (!PassesGuards(qtyToSell, price, _config.MinQty, _config.MinNotional))
				return Task.FromResult(Noop(asset));
			return Task.FromResult(Sell(asset, qtyToSell, currentWeight, _config.TargetWeight));
		}
	}

	private static TradingSolution Noop(string asset) =>
	new()
	{
		Asset = asset,
		Quantity = 0m,
		Type = SolutionType.Hold,
		Reason = "Within threshold band"
	};

	private static TradingSolution Buy(string asset, decimal quantity, decimal currentWeight, decimal targetWeight) =>
	new()
	{
		Asset = asset,
		Quantity = quantity,
		Type = SolutionType.Buy,
		Reason = $"Weight {currentWeight:P2} below target {targetWeight:P2}, buying to rebalance"
	};

	private static TradingSolution Sell(string asset, decimal quantity, decimal currentWeight, decimal targetWeight) =>
	new()
	{
		Asset = asset,
		Quantity = quantity,
		Type = SolutionType.Sell,
		Reason = $"Weight {currentWeight:P2} above target {targetWeight:P2}, selling to rebalance"
	};

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
