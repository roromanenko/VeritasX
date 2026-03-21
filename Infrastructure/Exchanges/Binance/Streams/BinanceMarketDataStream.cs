using Binance.Net.Clients;
using Core.Domain;
using Core.Interfaces;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using Infrastructure.Exchanges.Binance.Helpers;
using Microsoft.Extensions.Logging;
using BinanceNet = Binance.Net;

namespace Infrastructure.Exchanges.Binance.Streams;

/// <summary>
/// Manages a Binance WebSocket connection for a single exchange account,
/// providing real-time market data subscriptions for price tickers and candlestick streams.<br/>
/// Reconnection and heartbeat handling are managed automatically by the underlying Binance.Net socket client.
/// </summary>
public class BinanceMarketDataStream : IMarketDataStream
{
	private readonly BinanceSocketClient _socketClient;
	private readonly Dictionary<string, UpdateSubscription> _subscriptions = [];
	private readonly ILogger<BinanceMarketDataStream> _logger;

	public BinanceMarketDataStream(ExchangeConnection connection, ILogger<BinanceMarketDataStream> logger)
	{
		_socketClient = new BinanceSocketClient(opt =>
		{
			opt.ApiCredentials = new ApiCredentials(connection.ApiKey, connection.SecretKey);
			opt.Environment = connection.IsTestnet
				? BinanceNet.BinanceEnvironment.Testnet
				: BinanceNet.BinanceEnvironment.Live;
		});

		_logger = logger;
	}

	/// <summary>
	/// Subscribes to real-time mini-ticker updates for the given symbol,
	/// invoking the callback with the latest price on each update.
	/// </summary>
	/// <param name="symbol">The trading symbol to subscribe to, e.g. <c>BTCUSDT</c>.</param>
	/// <param name="onPrice">Async callback invoked with the latest price on each tick.</param>
	/// <param name="ct">Cancellation token.</param>
	/// <exception cref="InvalidOperationException">Thrown if the subscription request fails.</exception>
	public async Task SubscribeToTicker(string symbol, Func<decimal, Task> onPrice, CancellationToken ct = default)
	{
		var result = await _socketClient.SpotApi.ExchangeData
			.SubscribeToMiniTickerUpdatesAsync(symbol, async data =>
				await InvokeCallbackSafe(
					() => onPrice(data.Data.LastPrice),
					context: $"Ticker callback error for {symbol}"));

		RegisterSubscription(result, symbol, errorMessage: $"Failed to subscribe to ticker for {symbol}");
	}

	/// <summary>
	/// Subscribes to real-time kline updates for the given symbol and interval,
	/// invoking the callback only when a candlestick is marked as final (closed).
	/// </summary>
	/// <param name="symbol">The trading symbol to subscribe to, e.g. <c>BTCUSDT</c>.</param>
	/// <param name="interval">The candlestick interval, e.g. 1 minute, 1 hour.</param>
	/// <param name="onCandle">Async callback invoked with the completed <see cref="Candle"/> on each close.</param>
	/// <param name="ct">Cancellation token.</param>
	/// <exception cref="InvalidOperationException">Thrown if the subscription request fails.</exception>
	public async Task SubscribeToKline(string symbol, TimeSpan interval, Func<Candle, Task> onCandle, CancellationToken ct = default)
	{
		var binanceInterval = BinanceHelpers.ToKlineInterval(interval);

		var result = await _socketClient.SpotApi.ExchangeData
			.SubscribeToKlineUpdatesAsync(symbol, binanceInterval, async data =>
			{
				if (!data.Data.Data.Final) return;

				await InvokeCallbackSafe(
					() => onCandle(new Candle(
						data.Data.Data.OpenTime,
						data.Data.Data.OpenPrice,
						data.Data.Data.HighPrice,
						data.Data.Data.LowPrice,
						data.Data.Data.ClosePrice,
						data.Data.Data.Volume)),
					context: $"Kline callback error for {symbol}");
			});

		RegisterSubscription(result, symbol, errorMessage: $"Failed to subscribe to kline for {symbol}");
	}

	/// <summary>
	/// Unsubscribes from updates for the given symbol and removes it from the active subscriptions.<br/>
	/// Has no effect if the symbol is not currently subscribed.
	/// </summary>
	/// <param name="symbol">The trading symbol to unsubscribe from.</param>
	public async Task Unsubscribe(string symbol)
	{
		if (_subscriptions.TryGetValue(symbol, out var sub))
		{
			await _socketClient.UnsubscribeAsync(sub);
			_subscriptions.Remove(symbol);
		}
	}

	/// <summary>
	/// Unsubscribes from all active streams, clears internal state, and disposes the socket client.
	/// </summary>
	public async ValueTask DisposeAsync()
	{
		await _socketClient.UnsubscribeAllAsync();
		_subscriptions.Clear();
		_socketClient.Dispose();
	}

	#region Private helpers

	/// <summary>
	/// Invokes <paramref name="callback"/> and logs any exception without rethrowing,
	/// preventing stream errors from propagating to the Binance.Net socket handler.
	/// </summary>
	private async Task InvokeCallbackSafe(Func<Task> callback, string context)
	{
		try
		{
			await callback();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[BinanceMarketDataStream] {Context}", context);
		}
	}

	/// <summary>
	/// Throws <see cref="InvalidOperationException"/> if the subscription failed,
	/// otherwise stores the subscription under <paramref name="symbol"/>.
	/// </summary>
	private void RegisterSubscription(CallResult<UpdateSubscription> result, string symbol, string errorMessage)
	{
		if (!result.Success)
			throw new InvalidOperationException($"{errorMessage}: {result.Error}");

		_subscriptions[symbol] = result.Data;
	}

	#endregion
}
