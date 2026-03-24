using Core.Domain;
using Core.Interfaces;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Bson;
using Moq;

namespace Infrastructure.Tests.StatisticsTests;

public class PortfolioSnapshotServiceTests
{
	private readonly Mock<IUserService> _userServiceMock;
	private readonly Mock<IExchangeServiceFactory> _exchangeServiceFactoryMock;
	private readonly Mock<IPortfolioNormalizationService> _normalizationMock;
	private readonly Mock<IPriceProvider> _priceProviderMock;
	private readonly Mock<IStatisticsCache> _cacheMock;
	private readonly PortfolioSnapshotService _sut;

	private readonly string _userId = ObjectId.GenerateNewId().ToString();

	private static readonly Dictionary<string, decimal> EmptyPrices = [];

	public PortfolioSnapshotServiceTests()
	{
		_userServiceMock = new Mock<IUserService>();
		_exchangeServiceFactoryMock = new Mock<IExchangeServiceFactory>();
		_normalizationMock = new Mock<IPortfolioNormalizationService>();
		_priceProviderMock = new Mock<IPriceProvider>();
		_cacheMock = new Mock<IStatisticsCache>();

		// Cache always misses so the price provider is called
		_cacheMock
			.Setup(c => c.GetAsync<Dictionary<string, decimal>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((Dictionary<string, decimal>?)null);
		_cacheMock
			.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, decimal>>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		_sut = new PortfolioSnapshotService(
			_userServiceMock.Object,
			_exchangeServiceFactoryMock.Object,
			_normalizationMock.Object,
			_priceProviderMock.Object,
			_cacheMock.Object,
			NullLogger<PortfolioSnapshotService>.Instance);
	}

	[Fact]
	public async Task GetPortfolioSnapshot_WithSingleExchange_ReturnsCorrectEquity()
	{
		// Arrange
		var prices = new Dictionary<string, decimal> { ["BTCUSDT"] = 87000m };
		_priceProviderMock
			.Setup(p => p.GetAllPricesAsync("Binance", It.IsAny<CancellationToken>()))
			.ReturnsAsync(prices);

		var connection = new ExchangeConnection { ApiKey = "test-key", SecretKey = "test-secret" };
		_userServiceMock
			.Setup(u => u.GetAllExchangeConnections(_userId))
			.ReturnsAsync(new Dictionary<ExchangeName, ExchangeConnection>
			{
				{ ExchangeName.Binance, connection }
			});

		var exchangeServiceMock = new Mock<IExchangeService>();
		exchangeServiceMock
			.Setup(e => e.GetPortfolio(_userId, connection, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new Portfolio
			{
				UserId = _userId,
				Exchange = ExchangeName.Binance,
				Balances =
				[
					new Balance { Asset = "BTC", Free = 1m, Locked = 0m },
					new Balance { Asset = "USDT", Free = 5000m, Locked = 0m }
				]
			});

		_exchangeServiceFactoryMock
			.Setup(f => f.Create(ExchangeName.Binance))
			.Returns(exchangeServiceMock.Object);

		_normalizationMock
			.Setup(n => n.ToUsd(1m, "BTC", It.IsAny<IReadOnlyDictionary<string, decimal>>()))
			.Returns(87000m);
		_normalizationMock
			.Setup(n => n.ToUsd(5000m, "USDT", It.IsAny<IReadOnlyDictionary<string, decimal>>()))
			.Returns(5000m);

		// Act
		var result = await _sut.GetPortfolioSnapshotAsync(_userId);

		// Assert
		result.Should().HaveCount(1);
		result[0].EquityUsd.Should().Be(92000m);
		result[0].AllocationPercent.Should().Be(100m);
		result[0].Assets.Should().HaveCount(2);
	}

	[Fact]
	public async Task GetPortfolioSnapshot_WithZeroBalances_FiltersThemOut()
	{
		// Arrange
		_priceProviderMock
			.Setup(p => p.GetAllPricesAsync("Binance", It.IsAny<CancellationToken>()))
			.ReturnsAsync(EmptyPrices);

		var connection = new ExchangeConnection { ApiKey = "test-key", SecretKey = "test-secret" };
		_userServiceMock
			.Setup(u => u.GetAllExchangeConnections(_userId))
			.ReturnsAsync(new Dictionary<ExchangeName, ExchangeConnection>
			{
				{ ExchangeName.Binance, connection }
			});

		var exchangeServiceMock = new Mock<IExchangeService>();
		exchangeServiceMock
			.Setup(e => e.GetPortfolio(_userId, connection, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new Portfolio
			{
				UserId = _userId,
				Exchange = ExchangeName.Binance,
				Balances =
				[
					new Balance { Asset = "BTC", Free = 1m, Locked = 0m },
					new Balance { Asset = "ETH", Free = 0m, Locked = 0m }
				]
			});

		_exchangeServiceFactoryMock
			.Setup(f => f.Create(ExchangeName.Binance))
			.Returns(exchangeServiceMock.Object);

		_normalizationMock
			.Setup(n => n.ToUsd(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, decimal>>()))
			.Returns((decimal amount, string _, IReadOnlyDictionary<string, decimal> _) => amount * 1000m);

		// Act
		var result = await _sut.GetPortfolioSnapshotAsync(_userId);

		// Assert
		result[0].Assets.Should().HaveCount(1);
		result[0].Assets[0].Asset.Should().Be("BTC");
	}

	[Fact]
	public async Task GetPortfolioSnapshot_WhenExchangeFails_SkipsAndContinues()
	{
		// Arrange
		_priceProviderMock
			.Setup(p => p.GetAllPricesAsync("Binance", It.IsAny<CancellationToken>()))
			.ReturnsAsync(EmptyPrices);

		var connection = new ExchangeConnection { ApiKey = "test-key", SecretKey = "test-secret" };
		_userServiceMock
			.Setup(u => u.GetAllExchangeConnections(_userId))
			.ReturnsAsync(new Dictionary<ExchangeName, ExchangeConnection>
			{
				{ ExchangeName.Binance, connection }
			});

		var exchangeServiceMock = new Mock<IExchangeService>();
		exchangeServiceMock
			.Setup(e => e.GetPortfolio(_userId, connection, It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception("Exchange unavailable"));

		_exchangeServiceFactoryMock
			.Setup(f => f.Create(ExchangeName.Binance))
			.Returns(exchangeServiceMock.Object);

		// Act
		var act = async () => await _sut.GetPortfolioSnapshotAsync(_userId);

		// Assert
		await act.Should().NotThrowAsync();
		var result = await _sut.GetPortfolioSnapshotAsync(_userId);
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task GetPortfolioSnapshot_AssetsOrderedByUsdValueDescending()
	{
		// Arrange
		var prices = new Dictionary<string, decimal>
		{
			["BTCUSDT"] = 87000m, ["ETHUSDT"] = 2000m
		};
		_priceProviderMock
			.Setup(p => p.GetAllPricesAsync("Binance", It.IsAny<CancellationToken>()))
			.ReturnsAsync(prices);

		var connection = new ExchangeConnection { ApiKey = "test-key", SecretKey = "test-secret" };
		_userServiceMock
			.Setup(u => u.GetAllExchangeConnections(_userId))
			.ReturnsAsync(new Dictionary<ExchangeName, ExchangeConnection>
			{
				{ ExchangeName.Binance, connection }
			});

		var exchangeServiceMock = new Mock<IExchangeService>();
		exchangeServiceMock
			.Setup(e => e.GetPortfolio(_userId, connection, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new Portfolio
			{
				UserId = _userId,
				Exchange = ExchangeName.Binance,
				Balances =
				[
					new Balance { Asset = "ETH", Free = 10m, Locked = 0m },
					new Balance { Asset = "BTC", Free = 1m, Locked = 0m },
					new Balance { Asset = "USDT", Free = 100m, Locked = 0m }
				]
			});

		_exchangeServiceFactoryMock
			.Setup(f => f.Create(ExchangeName.Binance))
			.Returns(exchangeServiceMock.Object);

		_normalizationMock
			.Setup(n => n.ToUsd(10m, "ETH", It.IsAny<IReadOnlyDictionary<string, decimal>>()))
			.Returns(20000m);
		_normalizationMock
			.Setup(n => n.ToUsd(1m, "BTC", It.IsAny<IReadOnlyDictionary<string, decimal>>()))
			.Returns(87000m);
		_normalizationMock
			.Setup(n => n.ToUsd(100m, "USDT", It.IsAny<IReadOnlyDictionary<string, decimal>>()))
			.Returns(100m);

		// Act
		var result = await _sut.GetPortfolioSnapshotAsync(_userId);

		// Assert
		var assets = result[0].Assets;
		assets.Should().HaveCount(3);
		assets[0].Asset.Should().Be("BTC");
		assets[1].Asset.Should().Be("ETH");
		assets[2].Asset.Should().Be("USDT");
	}

	[Fact]
	public async Task GetPortfolioSnapshot_AllocationPercent_SumsTo100()
	{
		// Arrange
		_priceProviderMock
			.Setup(p => p.GetAllPricesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(EmptyPrices);

		var connectionA = new ExchangeConnection { ApiKey = "test-key", SecretKey = "test-secret" };
		var connectionB = new ExchangeConnection { ApiKey = "test-key", SecretKey = "test-secret" };

		_userServiceMock
			.Setup(u => u.GetAllExchangeConnections(_userId))
			.ReturnsAsync(new Dictionary<ExchangeName, ExchangeConnection>
			{
				{ ExchangeName.Binance, connectionA },
				{ ExchangeName.Bybit, connectionB }
			});

		var exchangeServiceA = new Mock<IExchangeService>();
		exchangeServiceA
			.Setup(e => e.GetPortfolio(_userId, connectionA, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new Portfolio
			{
				UserId = _userId,
				Exchange = ExchangeName.Binance,
				Balances = [new Balance { Asset = "USDT", Free = 3000m, Locked = 0m }]
			});

		var exchangeServiceB = new Mock<IExchangeService>();
		exchangeServiceB
			.Setup(e => e.GetPortfolio(_userId, connectionB, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new Portfolio
			{
				UserId = _userId,
				Exchange = ExchangeName.Bybit,
				Balances = [new Balance { Asset = "USDT", Free = 7000m, Locked = 0m }]
			});

		_exchangeServiceFactoryMock.Setup(f => f.Create(ExchangeName.Binance)).Returns(exchangeServiceA.Object);
		_exchangeServiceFactoryMock.Setup(f => f.Create(ExchangeName.Bybit)).Returns(exchangeServiceB.Object);

		_normalizationMock
			.Setup(n => n.ToUsd(It.IsAny<decimal>(), "USDT", It.IsAny<IReadOnlyDictionary<string, decimal>>()))
			.Returns((decimal amount, string _, IReadOnlyDictionary<string, decimal> _) => amount);

		// Act
		var result = await _sut.GetPortfolioSnapshotAsync(_userId);

		// Assert
		result.Should().HaveCount(2);
		var totalAllocation = result.Sum(s => s.AllocationPercent);
		totalAllocation.Should().BeApproximately(100m, 0.01m);
	}

	[Fact]
	public async Task GetPortfolioSnapshot_UsesCachedPrices_WhenCacheHit()
	{
		// Arrange
		var cachedPrices = new Dictionary<string, decimal> { ["BTCUSDT"] = 87000m };
		_cacheMock
			.Setup(c => c.GetAsync<Dictionary<string, decimal>>("stats:prices:Binance", It.IsAny<CancellationToken>()))
			.ReturnsAsync(cachedPrices);

		var connection = new ExchangeConnection { ApiKey = "test-key", SecretKey = "test-secret" };
		_userServiceMock
			.Setup(u => u.GetAllExchangeConnections(_userId))
			.ReturnsAsync(new Dictionary<ExchangeName, ExchangeConnection>
			{
				{ ExchangeName.Binance, connection }
			});

		var exchangeServiceMock = new Mock<IExchangeService>();
		exchangeServiceMock
			.Setup(e => e.GetPortfolio(_userId, connection, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new Portfolio
			{
				UserId = _userId,
				Exchange = ExchangeName.Binance,
				Balances = [new Balance { Asset = "BTC", Free = 1m, Locked = 0m }]
			});

		_exchangeServiceFactoryMock
			.Setup(f => f.Create(ExchangeName.Binance))
			.Returns(exchangeServiceMock.Object);

		_normalizationMock
			.Setup(n => n.ToUsd(1m, "BTC", It.IsAny<IReadOnlyDictionary<string, decimal>>()))
			.Returns(87000m);

		// Act
		await _sut.GetPortfolioSnapshotAsync(_userId);

		// Assert — price provider should NOT be called when cache hits
		_priceProviderMock.Verify(
			p => p.GetAllPricesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
			Times.Never);
	}
}
