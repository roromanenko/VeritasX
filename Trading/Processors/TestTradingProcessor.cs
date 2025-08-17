using Core.Interfaces;
using Trading.Strategies;

namespace Trading.Processors;

public class TestTradingProcessor : ITradingProcessor
{
	private readonly ICandleChunkService _candleChunkService;
	private readonly string _jobId;
	private readonly string _baseline;
	private readonly string _targetAsset;
	private readonly decimal _initBaselineQuantity;

	public TestTradingProcessor(ICandleChunkService candleChunkService,
		string jobId,
		string baseline,
		string targetAsset,
		decimal initBaselineQuantity)
	{
		_candleChunkService = candleChunkService;
		_jobId = jobId;
		_baseline = baseline;
		_targetAsset = targetAsset;
		_initBaselineQuantity = initBaselineQuantity;
	}

	public async Task Start(ITradingStrategy strategy, string strategyConfigurationJson, TradingContext context, CancellationToken cancellationToken)
	{
		var candles = await _candleChunkService.GetCandlesByJobIdAsync(_jobId);
		var testPriceProvider = new TestPriceProvider();
		context = new TradingContext(new AccountContext(_baseline), testPriceProvider);
		context.Account.SetBalance(_baseline, _initBaselineQuantity);
		var startTotal = await context.GetTotalInBaseline(cancellationToken);

		foreach (var candle in candles)
		{
			var currentPrice = candle.Close;
			testPriceProvider.SetPrice(currentPrice);
			var solution = await strategy.CalculateNextStep(strategyConfigurationJson, context, cancellationToken);

			switch (solution.Type)
			{
				case SolutionType.Buy:
					context.Account.AdjustBalance(_baseline, -solution.Quantity * currentPrice);
					context.Account.AdjustBalance(_targetAsset, solution.Quantity);
					break;
				case SolutionType.Sell:
					context.Account.AdjustBalance(_baseline, solution.Quantity * currentPrice);
					context.Account.AdjustBalance(_targetAsset, -solution.Quantity);
					break;
				case SolutionType.Hold:
					break;
			}
		}
		
		var endTotal = await context.GetTotalInBaseline(cancellationToken);
		Console.WriteLine($"Test completed. Start total: {startTotal}, End total: {endTotal}");
	}

	public Task Stop(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}
}
