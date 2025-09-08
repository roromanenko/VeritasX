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
				(baselineQty, targetQty) = (balances[_jobInfo.QuoteAsset], balances[_jobInfo.BaseAsset]);
				tradingStarted = true;
			}
		}
		
		var endTotal = await context.GetTotalInBaseline(cancellationToken);
		return new TradingResult
		{
			StartTotalInBaseline = startTotal,
			EndTotalInBaseline = endTotal,
			JustHoldTotalInBaseline = baselineQty + targetQty * currentPrice,
			ProfitInBaseline = endTotal - startTotal,
		};
	}

	public Task Stop(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}
}


public class TestPriceProvider : IPriceProvider
{
	private decimal _currentPrice = 0;

	public Task<IEnumerable<Candle>> GetHistoryAsync(string symbol, DateTimeOffset fromUtc, DateTimeOffset toUtc, TimeSpan interval, CancellationToken ct = default)
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