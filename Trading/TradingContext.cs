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
	private readonly IPriceProvider _prices;

	public AccountContext Account { get; }
	public TradingContext(AccountContext account, IPriceProvider prices)
	{
		Account = account ?? throw new ArgumentNullException(nameof(account));
		_prices = prices ?? throw new ArgumentNullException(nameof(prices));
	}

	public Task<decimal> GetPriceInBaselineAsync(string asset, CancellationToken ct = default)
	{
		asset = asset.ToUpperInvariant();
		return asset.Equals(Account.Baseline, StringComparison.OrdinalIgnoreCase)
			? Task.FromResult(1m)
			: _prices.GetPriceAsync(asset, Account.Baseline, ct);
	}

	public async Task<decimal> GetTotalInBaseline(CancellationToken ct)
	{
		var priceTasks = Account.GetBalances().Select(async item =>
		{
			decimal px = item.Key.Equals(Account.Baseline, StringComparison.OrdinalIgnoreCase)
				? 1m
				: await _prices.GetPriceAsync(item.Key, Account.Baseline, ct).ConfigureAwait(false);

			return item.Value * px;
		});

		var results = await Task.WhenAll(priceTasks).ConfigureAwait(false);	

		return results.Sum();
	}

	public async Task<decimal> GetAssetWeightAsync(string asset, CancellationToken ct)
	{
		if (string.IsNullOrWhiteSpace(asset))
			throw new ArgumentException("asset required", nameof(asset));

		asset = asset.ToUpperInvariant();
		var total = await GetTotalInBaseline(ct).ConfigureAwait(false);
		if (total == 0) return 0;
		var balance = Account.GetBalances().GetValueOrDefault(asset, 0);
		if (balance == 0) return 0;
		var price = asset.Equals(Account.Baseline, StringComparison.OrdinalIgnoreCase)
			? 1m
			: await _prices.GetPriceAsync(asset, Account.Baseline, ct).ConfigureAwait(false);
		return (balance * price) / total;
	}
}