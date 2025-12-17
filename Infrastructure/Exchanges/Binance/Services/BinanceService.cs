using AutoMapper;
using Binance.Net.Interfaces.Clients;
using Binance.Net.Objects.Models;
using Binance.Net.Objects.Models.Spot;
using Core.Domain;
using Infrastructure.Exchanges.Binance.Helpers;
using Infrastructure.Exchanges.Binance.Models;
using Microsoft.Extensions.Options;
using BinanceNet = Binance.Net;

namespace Infrastructure.Exchanges.Binance.Services
{
	public class BinanceService : Core.Interfaces.IExchangeService
	{
		private readonly IBinanceRestClient _client;
		private readonly BinanceConfig _config;
		private readonly IMapper _mapper;

		public BinanceService(IBinanceRestClient client, IOptions<BinanceConfig> config, IMapper mapper)
		{
			_client = client;
			_config = config.Value;
			_mapper = mapper;
		}

		/// <summary>
		/// Tests connectivity to the Binance API.
		/// </summary>
		/// <returns>
		/// True if connection is successful, otherwise false.
		/// </returns>
		public async Task<bool> TestConnectivity(CancellationToken cancellationToken = default)
		{
			var result = await _client.SpotApi.ExchangeData.PingAsync(cancellationToken);
			return result.Success;
		}

		/// <summary>
		/// Gets the current server time from Binance.
		/// </summary>
		/// <returns>
		/// Server time in Unix milliseconds, or 0 if request fails.
		/// </returns>
		public async Task<long> GetServerTime(CancellationToken cancellationToken = default)
		{
			var result = await _client.SpotApi.ExchangeData.GetServerTimeAsync(cancellationToken);
			return result.Success ? new DateTimeOffset(result.Data).ToUnixTimeMilliseconds() : 0;
		}

		/// <summary>
		/// Retrieves trading pair information including price and quantity constraints.
		/// </summary>
		/// <param name="symbol">
		/// Trading pair symbol (e.g., "BTCUSDT").
		/// </param>
		/// <returns>
		/// Trading pair details, or null if symbol not found.
		/// </returns>
		public async Task<TradingPair?> GetTradingPairInfo(string symbol, CancellationToken cancellationToken = default)
		{
			var result = await _client.SpotApi.ExchangeData.GetExchangeInfoAsync(symbol, ct: cancellationToken);

			if (!result.Success || result.Data.Symbols.Length == 0)
				return null;

			var binanceSymbol = result.Data.Symbols[0];

			return new TradingPair
			{
				Exchange = ExchangeName.Binance,
				Symbol = binanceSymbol.Name,
				BaseAsset = binanceSymbol.BaseAsset,
				QuoteAsset = binanceSymbol.QuoteAsset,
				MinQuantity = BinanceHelpers.GetMinQuantity(binanceSymbol),
				MaxQuantity = BinanceHelpers.GetMaxQuantity(binanceSymbol),
				QuantityStepSize = BinanceHelpers.GetStepSize(binanceSymbol),
				MinNotional = BinanceHelpers.GetMinNotional(binanceSymbol),
				MinPrice = BinanceHelpers.GetMinPrice(binanceSymbol),
				MaxPrice = BinanceHelpers.GetMaxPrice(binanceSymbol),
				PriceTickSize = BinanceHelpers.GetTickSize(binanceSymbol),
				IsActive = binanceSymbol.Status == BinanceNet.Enums.SymbolStatus.Trading,
				UpdatedAt = DateTimeOffset.UtcNow
			};
		}

		/// <summary>
		/// Gets the current price information for a trading pair.
		/// </summary>
		/// <param name="symbol">
		/// Trading pair symbol (e.g., "BTCUSDT").
		/// </param>
		/// <returns>
		/// Current price data including bid/ask prices, or null if request fails.
		/// </returns>
		public async Task<Price?> GetPrice(string symbol, CancellationToken cancellationToken = default)
		{
			var result = await _client.SpotApi.ExchangeData.GetTickerAsync(symbol, cancellationToken);

			if (!result.Success)
				return null;

			return new Price
			{
				Exchange = ExchangeName.Binance,
				Symbol = result.Data.Symbol,
				LastPrice = result.Data.LastPrice,
				BidPrice = result.Data.BestBidPrice,
				AskPrice = result.Data.BestAskPrice,
				Timestamp = DateTimeOffset.UtcNow
			};
		}

		/// <summary>
		/// Retrieves historical candlestick data for a trading pair.
		/// </summary>
		/// <param name="symbol">Trading pair symbol (e.g., "BTCUSDT").</param>
		/// <param name="interval">Candle interval (e.g., "1m", "1h", "1d").</param>
		/// <param name="startTime">Optional start time for data range.</param>
		/// <param name="endTime">Optional end time for data range.</param>
		/// <param name="limit">Maximum number of candles to retrieve (default: 500).</param>
		/// <returns>List of candles, or empty list if request fails.</returns>
		public async Task<List<Candle>> GetCandles(string symbol, string interval, DateTime? startTime = null, DateTime? endTime = null, int limit = 500, CancellationToken cancellationToken = default)
		{
			var klineInterval = BinanceHelpers.ParseInterval(interval);
			var result = await _client.SpotApi.ExchangeData.GetKlinesAsync(symbol, klineInterval, startTime, endTime, limit, cancellationToken);

			if (!result.Success)
				return new List<Candle>();

			return result.Data.Select(k => new Candle(
				OpenTimeUtc: k.OpenTime,
				Open: k.OpenPrice,
				High: k.HighPrice,
				Low: k.LowPrice,
				Close: k.ClosePrice,
				Volume: k.Volume
			)).ToList();
		}

		/// <summary>
		/// Retrieves the user's portfolio including all asset balances.
		/// </summary>
		/// <returns>
		/// Portfolio with all balances, or null if request fails.
		/// </returns>
		public async Task<Portfolio?> GetPortfolio(string userId, CancellationToken cancellationToken = default)
		{
			var result = await _client.SpotApi.Account.GetAccountInfoAsync(ct: cancellationToken);

			if (!result.Success)
				return null;

			return new Portfolio
			{
				UserId = userId,
				Exchange = ExchangeName.Binance,
				IsTestnet = _config.UseTestnet,
				UpdatedAt = DateTimeOffset.UtcNow,
				Balances = result.Data.Balances
					.Where(b => b.Available > 0 || b.Locked > 0)
					.Select(b => new Balance
					{
						Asset = b.Asset,
						Free = b.Available,
						Locked = b.Locked
					}).ToList()
			};
		}

		/// <summary>
		/// Places a new <see cref="Order"/> on Binance after validating quantity, price, and notional value.
		/// </summary>
		/// <param name="order">Order details to place.</param>
		/// <returns>Placed order with exchange ID, or null if validation or placement fails.</returns>
		public async Task<Order?> PlaceOrder(Order order, CancellationToken cancellationToken = default)
		{
			var symbolInfo = await GetTradingPairInfo(order.Symbol, cancellationToken);
			if (symbolInfo == null) 
			{
				return null;
			}

			var binanceSymbol = await GetBinanceSymbol(order.Symbol, cancellationToken);

			if (!BinanceHelpers.ValidateQuantity(order.Quantity, binanceSymbol))
			{
				return null;
			}

			if (order.Type == Core.Domain.OrderType.Limit)
			{
				if (!order.Price.HasValue)
				{
					return null;
				}

				if (!BinanceHelpers.ValidatePrice(order.Price.Value, binanceSymbol))
				{
					return null;
				}

				if (!BinanceHelpers.ValidateNotional(order.Price.Value, order.Quantity, binanceSymbol, false))
				{
					return null;
				}
			}

			var side = BinanceHelpers.FromDomainSide(order.Side);
			var type = BinanceHelpers.FromDomainOrderType(order.Type);

			var result = await _client.SpotApi.Trading.PlaceOrderAsync(
				symbol: order.Symbol,
				side: side,
				type: type,
				quantity: order.Quantity,
				price: order.Price,
				timeInForce: type == BinanceNet.Enums.SpotOrderType.Limit ? BinanceNet.Enums.TimeInForce.GoodTillCanceled : null,
				ct: cancellationToken
			);

			if (!result.Success)
				return null;

			var mappedOrder = _mapper.Map<Order>(result.Data);
			mappedOrder.UserId = order.UserId;
			mappedOrder.IsTestnet = order.IsTestnet;
			return mappedOrder;
		}

		/// <summary>
		/// Retrieves details of a specific order.
		/// </summary>
		/// <param name="symbol">Trading pair symbol.</param>
		/// <param name="orderId">Exchange order ID.</param>
		/// <returns>Order details, or null if order not found.</returns>
		public async Task<Order?> GetOrder(string symbol, long orderId, CancellationToken cancellationToken = default)
		{
			var result = await _client.SpotApi.Trading.GetOrderAsync(symbol, orderId, ct: cancellationToken);

			if (!result.Success)
				return null;

			var mappedOrder = _mapper.Map<Order>(result.Data);
			mappedOrder.IsTestnet = _config.UseTestnet;
			return mappedOrder;
		}

		public async Task<Order?> CancelOrder(string symbol, long orderId, CancellationToken cancellationToken = default)
		{
			var result = await _client.SpotApi.Trading.CancelOrderAsync(symbol, orderId, ct: cancellationToken);

			if (!result.Success)
				return null;

			var mappedOrder = _mapper.Map<Order>(result.Data);
			mappedOrder.IsTestnet = _config.UseTestnet;
			return mappedOrder;
		}

		/// <summary>
		/// Retrieves all open orders for a symbol or all symbols.
		/// </summary>
		/// <returns>List of open orders, or empty list if request fails.</returns>
		public async Task<List<Order>> GetOpenOrders(string? symbol = null, CancellationToken cancellationToken = default)
		{
			var result = await _client.SpotApi.Trading.GetOpenOrdersAsync(symbol, ct: cancellationToken);

			if (!result.Success)
				return new List<Order>();

			return result.Data.Select(o =>
			{
				var mappedOrder = _mapper.Map<Order>(o);
				mappedOrder.IsTestnet = _config.UseTestnet;
				return mappedOrder;
			}).ToList();
		}

		/// <summary>
		/// Retrieves trade history for a symbol.
		/// </summary>
		public async Task<List<Trade>> GetTrades(string symbol, string? userId = null, long? orderId = null, DateTime? startTime = null, DateTime? endTime = null, int limit = 500, CancellationToken cancellationToken = default)
		{
			var result = await _client.SpotApi.Trading.GetUserTradesAsync(
				symbol: symbol,
				orderId: orderId,
				startTime: startTime,
				endTime: endTime,
				limit: limit,
				ct: cancellationToken
			);

			if (!result.Success)
				return new List<Trade>();

			return result.Data.Select(t =>
			{
				var mappedTrade = _mapper.Map<Trade>(t);
				mappedTrade.UserId = userId;
				mappedTrade.IsTestnet = _config.UseTestnet;
				return mappedTrade;
			}).ToList();
		}

		/// <summary>
		/// Retrieves raw Binance symbol information for validation purposes.
		/// </summary>
		private async Task<BinanceSymbol?> GetBinanceSymbol(string symbol, CancellationToken cancellationToken)
		{
			var result = await _client.SpotApi.ExchangeData.GetExchangeInfoAsync(symbol, ct: cancellationToken);

			if (!result.Success || result.Data.Symbols.Length == 0)
				return null;

			return result.Data.Symbols[0];
		}
	}
}
