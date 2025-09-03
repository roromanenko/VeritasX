using Core.Domain;
using Core.Interfaces;
using Trading.Strategies;

namespace Trading.Processors;

public class TestTradingProcessor : ITradingProcessor
{
	private TradingContext _context;
	private readonly ICandleChunkService _candleChunkService;
	private readonly ITradingStrategy _strategy;
	private readonly string _jobId;
	private readonly string _baseline;
	private readonly string _targetAsset;
	private readonly decimal _initBaselineQuantity;

	public TestTradingProcessor(
		TradingContext context,
		ITradingStrategy strategy,
		ICandleChunkService candleChunkService,
		string jobId,
		string baseline,
		string targetAsset,
		decimal initBaselineQuantity)
	{
		_context = context;
		_candleChunkService = candleChunkService;
		_strategy = strategy;
		_jobId = jobId;
		_baseline = baseline;
		_targetAsset = targetAsset;
		_initBaselineQuantity = initBaselineQuantity;
	}

	public async Task Start(CancellationToken cancellationToken)
	{
		var candles = await _candleChunkService.GetCandlesByJobIdAsync(_jobId);
		var testPriceProvider = new TestPriceProvider();
		_context = new TradingContext(new AccountContext(_baseline), testPriceProvider);
		_context.Account.SetBalance(_baseline, _initBaselineQuantity);
		var startTotal = await _context.GetTotalInBaseline(cancellationToken);

		foreach (var candle in candles)
		{
			var currentPrice = candle.Close;
			testPriceProvider.SetPrice(currentPrice);
			var solution = await _strategy.CalculateNextStep(cancellationToken);

			switch (solution.Type)
			{
				case SolutionType.Buy:
					_context.Account.AdjustBalance(_baseline, -solution.Quantity * currentPrice);
					_context.Account.AdjustBalance(_targetAsset, solution.Quantity);
					break;
				case SolutionType.Sell:
					_context.Account.AdjustBalance(_baseline, solution.Quantity * currentPrice);
					_context.Account.AdjustBalance(_targetAsset, -solution.Quantity);
					break;
				case SolutionType.Hold:
					break;
			}
		}
		
		var endTotal = await _context.GetTotalInBaseline(cancellationToken);
		Console.WriteLine($"Test completed. Start total: {startTotal}, End total: {endTotal}");
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