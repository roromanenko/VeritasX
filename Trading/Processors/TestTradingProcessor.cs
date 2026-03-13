using Core.Domain;
using Core.Interfaces;
using Trading.Strategies;

namespace Trading.Processors;

public class TestTradingProcessor : ITradingProcessor
{
	private readonly ITradingStrategy _strategy;
	private readonly IEnumerable<Candle> _candles;
	private readonly DataCollectionJob _jobInfo;
	private readonly decimal _initBaselineQuantity;

	public TestTradingProcessor(
		ITradingStrategy strategy,
		IEnumerable<Candle> candels,
		DataCollectionJob jobInfo,
		decimal initBaselineQuantity)
	{
		_strategy = strategy;
		_candles = candels;
		_jobInfo = jobInfo;
		_initBaselineQuantity = initBaselineQuantity;
	}

	public async Task<TradingResult> Start(CancellationToken cancellationToken)
	{
		var testPriceProvider = new TestPriceProvider();
		var context = new TradingContext(new AccountContext(_jobInfo.QuoteAsset), testPriceProvider);
		context.Account.SetBalance(_jobInfo.QuoteAsset, _initBaselineQuantity);
		var startTotal = await context.GetTotalInBaseline(cancellationToken);
		bool tradingStarted = false;

		(decimal baselineQty, decimal targetQty) = (0, 0);
		decimal currentPrice = 0;

		var portfolioSnapshots = new List<decimal>();
		var holdSnapshots = new List<decimal>();

		foreach (var candle in _candles)
		{
			currentPrice = candle.Close;
			testPriceProvider.SetPrice(currentPrice);
			var solution = await _strategy.CalculateNextStep(context, cancellationToken);

			switch (solution.Type)
			{
				case SolutionType.Buy:
					context.Account.AdjustBalance(_jobInfo.QuoteAsset, -solution.Quantity * currentPrice);
					context.Account.AdjustBalance(_jobInfo.BaseAsset, solution.Quantity);
					break;
				case SolutionType.Sell:
					context.Account.AdjustBalance(_jobInfo.QuoteAsset, solution.Quantity * currentPrice);
					context.Account.AdjustBalance(_jobInfo.BaseAsset, -solution.Quantity);
					break;
				case SolutionType.Hold:
					break;
			}

			if (!tradingStarted)
			{
				var balances = context.Account.GetBalances();
				(baselineQty, targetQty) = (
					balances.GetValueOrDefault(_jobInfo.QuoteAsset, 0),
					balances.GetValueOrDefault(_jobInfo.BaseAsset, 0)
				);
				tradingStarted = true;
			}

			if (tradingStarted)
			{
				portfolioSnapshots.Add(await context.GetTotalInBaseline(cancellationToken));
				holdSnapshots.Add(baselineQty + targetQty * currentPrice);
			}
		}
		
		var endTotal = await context.GetTotalInBaseline(cancellationToken);
		var periodsPerYear = (int)(TimeSpan.FromDays(365) / _jobInfo.Interval);
		return new TradingResult
		{
			StartTotalInBaseline = startTotal,
			EndTotalInBaseline = endTotal,
			JustHoldTotalInBaseline = baselineQty + targetQty * currentPrice,
			ProfitInBaseline = endTotal - startTotal,
			PortfolioSnapshots = portfolioSnapshots,
			HoldSnapshots = holdSnapshots,
			SharpeStrategy = CalcSharpe(portfolioSnapshots, periodsPerYear),
			SharpeHold = CalcSharpe(holdSnapshots, periodsPerYear),
		};
	}

	public Task Stop(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Calculates the annualized Sharpe ratio from a series of equity snapshots.<br/>
	/// The Sharpe ratio measures risk-adjusted returns by comparing the mean return to its volatility.
	/// </summary>
	/// <param name="snapshots">Sequential equity snapshots (e.g., daily portfolio values).</param>
	/// <param name="periodsPerYear">Number of periods per year for annualization (e.g., 252 for daily trading days, 365 for daily data, 12 for monthly).</param>
	/// <returns>
	/// The annualized Sharpe ratio. Returns 0 if insufficient data (&lt;2 snapshots), 
	/// if variance is zero (constant returns), or if calculations encounter division by zero.
	/// </returns>
	private static decimal CalcSharpe(List<decimal> snapshots, int periodsPerYear)
	{
		if (snapshots.Count < 2) return 0;

		var returns = new List<decimal>();
		for (int i = 1; i < snapshots.Count; i++)
		{
			if (snapshots[i - 1] == 0) continue;
			returns.Add((snapshots[i] - snapshots[i - 1]) / snapshots[i - 1]);
		}

		if (returns.Count < 2) return 0;

		var mean = returns.Average();
		var variance = returns.Select(r => (r - mean) * (r - mean)).Average();
		var std = (decimal)Math.Sqrt((double)variance);

		if (std == 0) return 0;

		return mean / std * (decimal)Math.Sqrt(periodsPerYear);
	}
}

public class TestPriceProvider : IPriceProvider
{
	private decimal _currentPrice = 0;

	public Task<IEnumerable<Candle>> GetHistoryAsync(
		string asset,
		string baseline,
		DateTimeOffset fromUtc,
		DateTimeOffset toUtc,
		TimeSpan interval,
		CancellationToken ct = default)
	{
		return Task.FromResult(Enumerable.Empty<Candle>());
	}

	public Task<decimal> GetPriceAsync(string asset, string baseline, CancellationToken ct = default)
	{
		return Task.FromResult(_currentPrice);
	}

	public void SetPrice(decimal price)
	{
		_currentPrice = price;
	}
}
