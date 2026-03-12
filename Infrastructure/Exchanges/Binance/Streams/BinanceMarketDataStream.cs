using Binance.Net.Clients;
using Core.Domain;
using Core.Interfaces;
using CryptoExchange.Net.Authentication;
using Infrastructure.Exchanges.Binance.Helpers;
using BinanceNet = Binance.Net;

namespace Infrastructure.Exchanges.Binance.Streams;

internal class BinanceMarketDataStream : IMarketDataStream
{
	private readonly BinanceSocketClient _socketClient;
	private readonly List<string> _subscriptions = [];

	public BinanceMarketDataStream(ExchangeConnection connection)
	{
		_socketClient = new BinanceSocketClient(opt =>
		{
			opt.ApiCredentials = new ApiCredentials(connection.ApiKey, connection.SecretKey);
			opt.Environment = connection.IsTestnet
				? BinanceNet.BinanceEnvironment.Testnet
				: BinanceNet.BinanceEnvironment.Live;
		});
	}

	public async Task SubscribeToTickerAsync(string symbol, Action<decimal> onPrice, CancellationToken ct = default)
	{
		var result = await _socketClient.SpotApi.ExchangeData
			.SubscribeToMiniTickerUpdatesAsync(symbol, data =>
			{
				onPrice(data.Data.LastPrice);
			}, ct);

		if (!result.Success)
			throw new InvalidOperationException($"Failed to subscribe to ticker for {symbol}: {result.Error?.Message}");

		_subscriptions.Add(symbol);
	}

	public async Task SubscribeToKlineAsync(string symbol, TimeSpan interval, Action<Candle> onCandle, CancellationToken ct = default)
	{
		var binanceInterval = BinanceHelpers.ToKlineInterval(interval);

		var result = await _socketClient.SpotApi.ExchangeData
			.SubscribeToKlineUpdatesAsync(symbol, binanceInterval, data =>
			{
				var k = data.Data;
				if (!k.Data.Final) return;

				onCandle(new Candle(
					k.Data.OpenTime,
					k.Data.OpenPrice,
					k.Data.HighPrice,
					k.Data.LowPrice,
					k.Data.ClosePrice,
					k.Data.Volume
				));
			}, ct);

		if (!result.Success)
			throw new InvalidOperationException($"Failed to subscribe to klines for {symbol}: {result.Error?.Message}");

		_subscriptions.Add(symbol);
	}

	public async Task UnsubscribeAsync(string symbol)
	{
		await _socketClient.UnsubscribeAllAsync();
		_subscriptions.Remove(symbol);
	}

	public async ValueTask DisposeAsync()
	{
		await _socketClient.UnsubscribeAllAsync();
		_socketClient.Dispose();
	}
}
