using AutoMapper;
using Core.Domain;
using Core.Interfaces;
using Infrastructure.Hubs;
using Infrastructure.Interfaces;
using Infrastructure.Persistence.Entities;
using Infrastructure.Trading;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Moq;
using Trading;
using Trading.Strategies;

namespace Infrastructure.Tests.TradingTests;

public class BotRunnerTests
{
	private readonly Mock<IServiceScope> _scopeMock = new();
	private readonly Mock<IBotRepository> _botRepositoryMock = new();
	private readonly Mock<IBotTradeRepository> _botTradeRepositoryMock = new();
	private readonly Mock<IUserService> _userServiceMock = new();
	private readonly Mock<IMarketDataStreamFactory> _streamFactoryMock = new();
	private readonly Mock<IMarketDataStream> _streamMock = new();
	private readonly Mock<ITradeExecutor> _tradeExecutorMock = new();
	private readonly Mock<IExchangeServiceFactory> _exchangeServiceFactoryMock = new();
	private readonly Mock<IExchangeService> _exchangeServiceMock = new();
	private readonly Mock<IStrategyFactory> _strategyFactoryMock = new();
	private readonly Mock<ITradingStrategy> _strategyMock = new();
	private readonly Mock<IHubContext<BotProgressHub>> _hubContextMock = new();
	private readonly Mock<IHubClients> _hubClientsMock = new();
	private readonly Mock<IClientProxy> _clientProxyMock = new();
	private readonly Mock<IMapper> _mapperMock = new();
	private readonly Mock<ILogger<BotRunner>> _loggerMock = new();

	private readonly BotConfiguration _bot;
	private readonly ObjectId _botObjectId;

	public BotRunnerTests()
	{
		_botObjectId = ObjectId.GenerateNewId();

		_bot = new BotConfiguration
		{
			Id = _botObjectId.ToString(),
			UserId = ObjectId.GenerateNewId().ToString(),
			Name = "Test Bot",
			Exchange = ExchangeName.Binance,
			Symbol = "BTCUSDT",
			BaseAsset = "BTC",
			QuoteAsset = "USDT",
			Strategy = new StrategyDefinition
			{
				Type = StrategyType.DeltaRebalancing,
				Parameters = []
			},
			RiskParameters = new RiskParameters { PositionSize = 0.1m }
		};

		// Hub mock chain
		_clientProxyMock
			.Setup(cp => cp.SendCoreAsync(
				It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);
		_hubClientsMock.Setup(hc => hc.Group(It.IsAny<string>())).Returns(_clientProxyMock.Object);
		_hubContextMock.Setup(h => h.Clients).Returns(_hubClientsMock.Object);

		// Stream factory/stream
		_streamFactoryMock
			.Setup(f => f.Create(It.IsAny<ExchangeName>(), It.IsAny<ExchangeConnection>()))
			.Returns(_streamMock.Object);
		_streamMock.Setup(s => s.Unsubscribe(It.IsAny<string>())).Returns(Task.CompletedTask);
		_streamMock.Setup(s => s.DisposeAsync()).Returns(ValueTask.CompletedTask);

		// Exchange service factory
		_exchangeServiceFactoryMock
			.Setup(f => f.Create(It.IsAny<ExchangeName>()))
			.Returns(_exchangeServiceMock.Object);

		// Strategy — returns Ticker data requirement and Hold by default
		_strategyMock.SetupGet(s => s.DataRequirement).Returns(DataRequirement.Ticker);
		_strategyMock
			.Setup(s => s.CalculateNextStep(
				It.IsAny<TradingContext>(), It.IsAny<MarketTick>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new TradingSolution { Asset = "BTC", Type = SolutionType.Hold });
		_strategyFactoryMock
			.Setup(f => f.Create(It.IsAny<StrategyDefinition>()))
			.Returns(_strategyMock.Object);

		// User service
		_userServiceMock
			.Setup(u => u.GetExchangeConnection(It.IsAny<string>(), It.IsAny<ExchangeName>()))
			.ReturnsAsync(new ExchangeConnection { ApiKey = "test_key", SecretKey = "test_secret" });

		// Bot repository — returns a fresh doc on each call so SetStatusAsync can mutate it freely
		_botRepositoryMock
			.Setup(r => r.GetBotById(_botObjectId))
			.ReturnsAsync(() => new BotConfigurationDocument { Id = _botObjectId, Status = BotStatus.Stopped });
		_botRepositoryMock
			.Setup(r => r.UpdateBot(It.IsAny<BotConfigurationDocument>()))
			.Returns(Task.CompletedTask);
	}

	private BotRunner CreateRunner() => new(
		_scopeMock.Object,
		_bot,
		_botRepositoryMock.Object,
		_botTradeRepositoryMock.Object,
		_userServiceMock.Object,
		_streamFactoryMock.Object,
		_tradeExecutorMock.Object,
		_exchangeServiceFactoryMock.Object,
		_strategyFactoryMock.Object,
		_hubContextMock.Object,
		_mapperMock.Object,
		_loggerMock.Object);

	/// <summary>
	/// Starts the runner in the background and returns both the running task and the captured
	/// ticker callback, which tests use to simulate individual market ticks.
	/// </summary>
	private async Task<(Task BotTask, Func<decimal, Task> TickCallback)> StartRunnerAsync(
		BotRunner runner, CancellationTokenSource cts)
	{
		var callbackTcs = new TaskCompletionSource<Func<decimal, Task>>();
		_streamMock
			.Setup(s => s.SubscribeToTicker(
				It.IsAny<string>(),
				It.IsAny<Func<decimal, Task>>(),
				It.IsAny<CancellationToken>()))
			.Callback<string, Func<decimal, Task>, CancellationToken>((_, cb, _) => callbackTcs.TrySetResult(cb))
			.Returns(Task.CompletedTask);

		var botTask = Task.Run(() => runner.StartAsync(cts.Token));
		var tickCallback = await callbackTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
		return (botTask, tickCallback);
	}

	[Fact]
	public async Task OnTick_WhenErrorsBelowThreshold_BotContinues()
	{
		//Arrange
		_exchangeServiceMock
			.Setup(e => e.GetPortfolio(
				It.IsAny<string>(), It.IsAny<ExchangeConnection>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("Exchange unreachable"));

		using var cts = new CancellationTokenSource();
		var runner = CreateRunner();
		var (_, tickCallback) = await StartRunnerAsync(runner, cts);

		//Act — fire 4 consecutive error ticks, one below the threshold of 5
		for (int i = 0; i < 4; i++)
			await tickCallback(100m);

		//Assert — bot did not self-stop
		_botRepositoryMock.Verify(
			r => r.UpdateBot(It.Is<BotConfigurationDocument>(d => d.Status == BotStatus.Error)),
			Times.Never);

		await cts.CancelAsync();
	}

	[Fact]
	public async Task OnTick_WhenConsecutiveErrorsReachThreshold_BotStops()
	{
		//Arrange
		_exchangeServiceMock
			.Setup(e => e.GetPortfolio(
				It.IsAny<string>(), It.IsAny<ExchangeConnection>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("Exchange unreachable"));

		using var cts = new CancellationTokenSource();
		var runner = CreateRunner();
		var (botTask, tickCallback) = await StartRunnerAsync(runner, cts);

		//Act — fire 5 consecutive error ticks, reaching MaxConsecutiveErrors
		for (int i = 0; i < 5; i++)
			await tickCallback(100m);

		//Assert — bot stopped itself and persisted Error status
		await botTask.WaitAsync(TimeSpan.FromSeconds(5));

		_botRepositoryMock.Verify(
			r => r.UpdateBot(It.Is<BotConfigurationDocument>(d => d.Status == BotStatus.Error)),
			Times.Once);
	}

	[Fact]
	public async Task OnTick_WhenSuccessfulTickAfterErrors_ResetsConsecutiveErrors()
	{
		//Arrange — toggle: start throwing, switch to success, then throw again
		var shouldThrow = true;
		_exchangeServiceMock
			.Setup(e => e.GetPortfolio(
				It.IsAny<string>(), It.IsAny<ExchangeConnection>(), It.IsAny<CancellationToken>()))
			.Returns(() =>
			{
				if (shouldThrow)
					return Task.FromException<Portfolio>(new InvalidOperationException("Exchange unreachable"));
				return Task.FromResult(new Portfolio
				{
					UserId = _bot.UserId,
					Exchange = _bot.Exchange,
					Balances = [new Balance { Asset = "USDT", Free = 1000m }]
				});
			});

		using var cts = new CancellationTokenSource();
		var runner = CreateRunner();
		var (_, tickCallback) = await StartRunnerAsync(runner, cts);

		//Act — 3 errors raise the counter, 1 success resets it, 4 more errors stay below threshold
		for (int i = 0; i < 3; i++)
			await tickCallback(100m);

		shouldThrow = false;
		await tickCallback(100m); // successful tick — counter resets to 0

		shouldThrow = true;
		for (int i = 0; i < 4; i++)
			await tickCallback(100m); // 4 new consecutive errors, still below threshold

		//Assert — bot did not stop; the reset prevented reaching the threshold
		_botRepositoryMock.Verify(
			r => r.UpdateBot(It.Is<BotConfigurationDocument>(d => d.Status == BotStatus.Error)),
			Times.Never);

		await cts.CancelAsync();
	}
}
