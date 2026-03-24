using Core.Domain.Statistics;
using Core.Interfaces;
using FluentAssertions;
using Infrastructure.Interfaces;
using Infrastructure.Persistence.Entities;
using Infrastructure.Services;
using MongoDB.Bson;
using Moq;

namespace Infrastructure.Tests.StatisticsTests;

public class BotStatisticsServiceTests
{
	private readonly Mock<IBotStatisticsRepository> _statisticsRepositoryMock;
	private readonly Mock<IBotRepository> _botRepositoryMock;
	private readonly Mock<IStatisticsCache> _cacheMock;
	private readonly BotStatisticsService _sut;

	private readonly string _botId = ObjectId.GenerateNewId().ToString();
	private readonly string _userId = ObjectId.GenerateNewId().ToString();

	public BotStatisticsServiceTests()
	{
		_statisticsRepositoryMock = new Mock<IBotStatisticsRepository>();
		_botRepositoryMock = new Mock<IBotRepository>();
		_cacheMock = new Mock<IStatisticsCache>();

		_sut = new BotStatisticsService(
			_statisticsRepositoryMock.Object,
			_botRepositoryMock.Object,
			_cacheMock.Object);

		// Default: cache miss
		_cacheMock
			.Setup(c => c.GetAsync<BotStatistics>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((BotStatistics?)null);
		_cacheMock
			.Setup(c => c.GetAsync<decimal?>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((decimal?)null);
		_cacheMock
			.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<BotStatistics>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		// Default: bot exists for this user
		_botRepositoryMock
			.Setup(r => r.GetBotById(It.IsAny<ObjectId>(), It.IsAny<ObjectId>()))
			.ReturnsAsync(new BotConfigurationDocument { Id = ObjectId.Parse(_botId), Symbol = "BTCUSDT" });
	}

	[Fact]
	public async Task GetBotStatistics_WhenLessThan30Days_SharpeIsNull()
	{
		// Arrange
		var snapshots = BuildSnapshots(29);
		_statisticsRepositoryMock
			.Setup(r => r.GetSnapshotsAsync(It.IsAny<ObjectId>(), null, null))
			.ReturnsAsync(snapshots);

		// Act
		var result = await _sut.GetBotStatisticsAsync(_botId, _userId, null, null);

		// Assert
		result.Should().NotBeNull();
		result!.Sharpe.Should().BeNull();
	}

	[Fact]
	public async Task GetBotStatistics_WhenAtLeast30Days_SharpeIsCalculated()
	{
		// Arrange — 31 days of monotonically increasing equity (non-zero daily returns)
		var snapshots = BuildSnapshots(31);
		_statisticsRepositoryMock
			.Setup(r => r.GetSnapshotsAsync(It.IsAny<ObjectId>(), null, null))
			.ReturnsAsync(snapshots);

		// Act
		var result = await _sut.GetBotStatisticsAsync(_botId, _userId, null, null);

		// Assert
		result.Should().NotBeNull();
		result!.Sharpe.Should().NotBeNull();
	}

	[Fact]
	public async Task GetBotStatistics_MaxDrawdown_IsCorrectForKnownData()
	{
		// Arrange — equities [100, 120, 80, 110] → peak=120, trough=80, MDD=(120-80)/120
		var base_ = new DateOnly(2026, 1, 1);
		var equities = new[] { 100m, 120m, 80m, 110m };
		var snapshots = equities.Select((e, i) => new BotDailyStatisticsDocument
		{
			BotId = ObjectId.Parse(_botId),
			Date = base_.AddDays(i),
			ClosingEquity = e,
			OpeningEquity = e
		}).ToList();

		_statisticsRepositoryMock
			.Setup(r => r.GetSnapshotsAsync(It.IsAny<ObjectId>(), null, null))
			.ReturnsAsync(snapshots);

		// Act
		var result = await _sut.GetBotStatisticsAsync(_botId, _userId, null, null);

		// Assert
		result.Should().NotBeNull();
		var expectedMdd = (120m - 80m) / 120m;
		result!.MaxDrawdownPercent.Should().BeApproximately(expectedMdd, 0.0001m);
	}

	[Fact]
	public async Task GetBotStatistics_WhenNoRoundTrips_WinRateIsZero()
	{
		// Arrange — snapshots with WinCount=0, LossCount=0
		var snapshots = BuildSnapshots(5);
		_statisticsRepositoryMock
			.Setup(r => r.GetSnapshotsAsync(It.IsAny<ObjectId>(), null, null))
			.ReturnsAsync(snapshots);

		// Act
		var result = await _sut.GetBotStatisticsAsync(_botId, _userId, null, null);

		// Assert
		result.Should().NotBeNull();
		result!.WinRate.Should().Be(0m);
	}

	[Fact]
	public async Task GetBotStatistics_WhenNoLosses_ProfitFactorIsNull()
	{
		// Arrange — WinCount>0 but GrossLoss=0
		var base_ = new DateOnly(2026, 1, 1);
		var snapshots = new List<BotDailyStatisticsDocument>
		{
			new()
			{
				BotId = ObjectId.Parse(_botId),
				Date = base_,
				ClosingEquity = 110m,
				OpeningEquity = 100m,
				WinCount = 3,
				LossCount = 0,
				GrossProfit = 30m,
				GrossLoss = 0m
			}
		};
		_statisticsRepositoryMock
			.Setup(r => r.GetSnapshotsAsync(It.IsAny<ObjectId>(), null, null))
			.ReturnsAsync(snapshots);

		// Act
		var result = await _sut.GetBotStatisticsAsync(_botId, _userId, null, null);

		// Assert
		result.Should().NotBeNull();
		result!.ProfitFactor.Should().BeNull();
	}

	[Fact]
	public async Task GetBotStatistics_SecondCall_ReturnsCachedResult()
	{
		// Arrange — first call returns null (cache miss), second call returns cached data
		var snapshots = BuildSnapshots(5);
		_statisticsRepositoryMock
			.Setup(r => r.GetSnapshotsAsync(It.IsAny<ObjectId>(), null, null))
			.ReturnsAsync(snapshots);

		BotStatistics? capturedStats = null;
		_cacheMock
			.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<BotStatistics>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
			.Callback<string, BotStatistics, TimeSpan, CancellationToken>((_, v, _, _) => capturedStats = v)
			.Returns(Task.CompletedTask);

		// First call — populates cache
		await _sut.GetBotStatisticsAsync(_botId, _userId, null, null);

		// Return cached value on second call
		_cacheMock
			.Setup(c => c.GetAsync<BotStatistics>($"stats:botstats:{_botId}", It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => capturedStats);

		// Act — second call
		await _sut.GetBotStatisticsAsync(_botId, _userId, null, null);

		// Assert — repository called exactly once across both calls
		_statisticsRepositoryMock.Verify(
			r => r.GetSnapshotsAsync(It.IsAny<ObjectId>(), null, null),
			Times.Once);
	}

	[Fact]
	public async Task GetBotStatistics_WhenCacheEmpty_LoadsFromRepository()
	{
		// Arrange — cache always misses
		var snapshots = BuildSnapshots(3);
		_statisticsRepositoryMock
			.Setup(r => r.GetSnapshotsAsync(It.IsAny<ObjectId>(), null, null))
			.ReturnsAsync(snapshots);

		// Act
		var result = await _sut.GetBotStatisticsAsync(_botId, _userId, null, null);

		// Assert
		result.Should().NotBeNull();
		_statisticsRepositoryMock.Verify(
			r => r.GetSnapshotsAsync(It.IsAny<ObjectId>(), null, null),
			Times.Once);
	}

	private List<BotDailyStatisticsDocument> BuildSnapshots(int count)
	{
		var base_ = new DateOnly(2026, 1, 1);
		return Enumerable.Range(0, count)
			.Select(i => new BotDailyStatisticsDocument
			{
				BotId = ObjectId.Parse(_botId),
				Date = base_.AddDays(i),
				ClosingEquity = 100m + i,
				OpeningEquity = 100m + i
			})
			.ToList();
	}
}
