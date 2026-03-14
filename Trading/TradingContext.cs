using Core.Interfaces;
using System.Collections.Concurrent;

namespace Trading;

public sealed class AccountContext
{
	public string Baseline { get; }

	private readonly ConcurrentDictionary<string, decimal> _balances =
		new(StringComparer.OrdinalIgnoreCase);

	public AccountContext(string baseline)
	{
		if (string.IsNullOrWhiteSpace(baseline)) throw new ArgumentException("baseline required");
		Baseline = baseline.ToUpperInvariant();
	}

	/// Upsert absolute balance (idempotent). Use AdjustBalance for deltas if you prefer.
	public void SetBalance(string asset, decimal quantity)
	{
		if (quantity < 0) throw new ArgumentOutOfRangeException(nameof(quantity));
		_balances.AddOrUpdate(asset.ToUpperInvariant(), quantity, (_, _) => quantity);
	}

	/// Add delta to balance (can be negative; result clamped at 0).
	public void AdjustBalance(string asset, decimal delta)
	{
		asset = asset.ToUpperInvariant();
		_balances.AddOrUpdate(asset, _ => Math.Max(0, delta), (_, cur) =>
		{
			var next = cur + delta;
			return next < 0 ? 0 : next;
		});
	}

	/// Read-only current balances (asset → qty).
	public IReadOnlyDictionary<string, decimal> GetBalances() =>
		new Dictionary<string, decimal>(_balances, StringComparer.OrdinalIgnoreCase);
}

public class TradingContext
{
	public AccountContext Account { get; }

	public TradingContext(AccountContext account)
	{
		Account = account ?? throw new ArgumentNullException(nameof(account));
	}

	public decimal GetPriceInBaseline(string asset, decimal currentPrice)
	{
		return asset.Equals(Account.Baseline, StringComparison.OrdinalIgnoreCase)
			? 1m
			: currentPrice;
	}

	public decimal GetTotalInBaseline(decimal currentPrice)
	{
		return Account.GetBalances().Sum(b =>
			b.Key.Equals(Account.Baseline, StringComparison.OrdinalIgnoreCase)
				? b.Value
				: b.Value * currentPrice);
	}

	public decimal GetAssetWeight(string asset, decimal currentPrice)
	{
		var total = GetTotalInBaseline(currentPrice);
		if (total == 0) return 0;

		var balance = Account.GetBalances().GetValueOrDefault(asset, 0);
		if (balance == 0) return 0;

		var price = GetPriceInBaseline(asset, currentPrice);
		return (balance * price) / total;
	}
}
