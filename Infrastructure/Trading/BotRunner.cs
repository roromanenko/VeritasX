using AutoMapper;
using Core.Domain;
using Core.Interfaces;
using Infrastructure.Hubs;
using Infrastructure.Interfaces;
using Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Trading;
using Trading.Strategies;

namespace Infrastructure.Trading;

public class BotRunner : IBotRunner
{
	public string BotId => _bot.Id;

	private readonly IServiceScope _scope;
	private readonly BotConfiguration _bot;
	private readonly IBotRepository _botRepository;
	private readonly IBotTradeRepository _botTradeRepository;
	private readonly IUserService _userService;
	private readonly IMarketDataStreamFactory _streamFactory;
	private readonly ITradeExecutor _tradeExecutor;
	private readonly IExchangeServiceFactory _exchangeServiceFactory;
	private readonly IStrategyFactory _strategyFactory;
	private readonly IHubContext<BotProgressHub> _hub;
	private readonly IMapper _mapper;
	private readonly ILogger<BotRunner> _logger;
	private readonly SemaphoreSlim _tickSemaphore = new(1, 1);

	private IMarketDataStream? _stream;
	private CancellationTokenSource? _cts;

	public BotRunner(
		IServiceScope scope,
		BotConfiguration bot,
		IBotRepository botRepository,
		IBotTradeRepository botTradeRepository,
		IUserService userService,
		IMarketDataStreamFactory streamFactory,
		ITradeExecutor tradeExecutor,
		IExchangeServiceFactory exchangeServiceFactory,
		IStrategyFactory strategyFactory,
		IHubContext<BotProgressHub> hub,
		IMapper mapper,
		ILogger<BotRunner> logger)
	{
		_scope = scope;
		_bot = bot;
		_botRepository = botRepository;
		_botTradeRepository = botTradeRepository;
		_userService = userService;
		_streamFactory = streamFactory;
		_tradeExecutor = tradeExecutor;
		_exchangeServiceFactory = exchangeServiceFactory;
		_strategyFactory = strategyFactory;
		_hub = hub;
		_mapper = mapper;
		_logger = logger;
	}

	public async Task StartAsync(CancellationToken ct)
	{
		_cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
		var token = _cts.Token;

		try
		{
			var connection = await _userService.GetExchangeConnection(_bot.UserId, _bot.Exchange);
			var strategy = _strategyFactory.Create(_bot.Strategy);
			var context = new TradingContext(new AccountContext(_bot.QuoteAsset));

			_stream = _streamFactory.Create(_bot.Exchange, connection);

			await SetStatusAsync(BotStatus.Active);
			await NotifyStatusAsync(BotStatus.Active);

			_logger.LogInformation(
				"Bot {BotId} started. Strategy: {Strategy}, Symbol: {Symbol}",
				BotId, _bot.Strategy.Type, _bot.Symbol);

			if (strategy.DataRequirement == DataRequirement.Ticker)
			{
				await _stream.SubscribeToTicker(_bot.Symbol, async price =>
				{
					var tick = new MarketTick
					{
						Symbol = _bot.Symbol,
						Price = price,
						Timestamp = DateTimeOffset.UtcNow
					};
					await OnTickAsync(tick, connection, strategy, context, token);
				}, token);
			}
			else
			{
				var interval = GetKlineInterval(_bot.Strategy);
				await _stream.SubscribeToKline(_bot.Symbol, interval, async candle =>
				{
					var tick = new MarketTick
					{
						Symbol = _bot.Symbol,
						Price = candle.Close,
						Candle = candle,
						Timestamp = candle.OpenTimeUtc
					};
					await OnTickAsync(tick, connection, strategy, context, token);
				}, token);
			}

			await Task.Delay(Timeout.Infinite, token);
		}
		catch (OperationCanceledException)
		{
			_logger.LogInformation("Bot {BotId} stopped.", BotId);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Bot {BotId} crashed.", BotId);
			await SetStatusAsync(BotStatus.Error, ex.Message);
			await NotifyStatusAsync(BotStatus.Error, ex.Message);
			throw;
		}
		finally
		{
			await ShutdownAsync();
		}
	}

	public async Task StopAsync()
	{
		if (_cts is not null)
			await _cts.CancelAsync();
	}

	public async ValueTask DisposeAsync()
	{
		_scope.Dispose();
		_tickSemaphore.Dispose();
	}

	private async Task OnTickAsync(
		MarketTick tick,
		ExchangeConnection connection,
		ITradingStrategy strategy,
		TradingContext context,
		CancellationToken ct)
	{
		if (ct.IsCancellationRequested)
			return;

		if (!await _tickSemaphore.WaitAsync(0, ct))
		{
			_logger.LogInformation("Bot with Id {BotId} skip tick processing", BotId);
			return;
		}

		try
		{
			var exchangeService = _exchangeServiceFactory.Create(_bot.Exchange);
			var portfolio = await exchangeService.GetPortfolio(_bot.UserId, connection, ct);

			var baseAsset = portfolio.Balances
				.FirstOrDefault(b => b.Asset.Equals(_bot.BaseAsset, StringComparison.OrdinalIgnoreCase));
			var quoteAsset = portfolio.Balances
				.FirstOrDefault(b => b.Asset.Equals(_bot.QuoteAsset, StringComparison.OrdinalIgnoreCase));

			context.Account.SetBalance(_bot.BaseAsset, baseAsset?.Free ?? 0);
			context.Account.SetBalance(_bot.QuoteAsset, quoteAsset?.Free ?? 0);

			var solution = await strategy.CalculateNextStep(context, tick, ct);

			if (solution.Type == SolutionType.Hold)
				return;

			var record = await _tradeExecutor.ExecuteAsync(solution, _bot, ct);
			if (record is null)
				return;

			var tradeDoc = _mapper.Map<BotTradeRecordDocument>(record);
			await _botTradeRepository.CreateTradeRecord(tradeDoc);

			await NotifyTradeAsync(record);

			_logger.LogInformation(
				"Bot {BotId} executed {Side} {Qty} {Asset} at {Price}. Reason: {Reason}",
				BotId, record.Side, record.Quantity, _bot.BaseAsset, record.Price, record.Reason);
		}
		catch (OperationCanceledException) { }
		catch (Exception ex)
		{
			_logger.LogError(ex, "Bot {BotId} error on tick.", BotId);
		}
		finally
		{
			_tickSemaphore.Release();
		}
	}

	private async Task ShutdownAsync()
	{
		try
		{
			if (_stream is not null)
			{
				await _stream.Unsubscribe(_bot.Symbol);
				await _stream.DisposeAsync();
				_stream = null;
			}
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Bot {BotId} error during stream shutdown.", BotId);
		}

		try
		{
			var current = await _botRepository.GetBotById(ObjectId.Parse(BotId));
			if (current is not null && current.Status != BotStatus.Error)
				await SetStatusAsync(BotStatus.Stopped);

			await NotifyStatusAsync(BotStatus.Stopped);
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Bot {BotId} error updating status on shutdown.", BotId);
		}
	}

	private async Task SetStatusAsync(BotStatus status, string? errorMessage = null)
	{
		var doc = await _botRepository.GetBotById(ObjectId.Parse(BotId));
		if (doc is null) return;

		doc.Status = status;
		doc.ErrorMessage = errorMessage;

		if (status == BotStatus.Active)
			doc.StartedAt = DateTimeOffset.UtcNow;
		else if (status is BotStatus.Stopped or BotStatus.Error)
			doc.StoppedAt = DateTimeOffset.UtcNow;

		await _botRepository.UpdateBot(doc);
	}

	private Task NotifyStatusAsync(BotStatus status, string? error = null)
	{
		return _hub.Clients.Group($"bot-{BotId}").SendAsync("BotStatusChanged", new
		{
			botId = BotId,
			status = status.ToString(),
			error,
			timestamp = DateTimeOffset.UtcNow
		});
	}

	private Task NotifyTradeAsync(BotTradeRecord record)
	{
		return _hub.Clients.Group($"bot-{BotId}").SendAsync("TradeExecuted", new
		{
			record.Id,
			record.Symbol,
			side = record.Side.ToString(),
			record.Price,
			record.Quantity,
			record.Reason,
			record.ExecutedAt
		});
	}

	private static TimeSpan GetKlineInterval(StrategyDefinition strategy)
	{
		if (strategy.Parameters.TryGetValue("interval", out var raw)
			&& TimeSpan.TryParse(raw, out var interval))
			return interval;

		return TimeSpan.FromHours(1);
	}
}
