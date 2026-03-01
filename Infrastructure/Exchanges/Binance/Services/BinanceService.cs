using AutoMapper;
using Binance.Net.Interfaces.Clients;
using Binance.Net.Objects.Models.Spot;
using Core.Domain;
using Core.Exceptions;
using Infrastructure.Exchanges.Binance.Factory;
using Infrastructure.Exchanges.Binance.Helpers;
using Microsoft.Extensions.Options;
using BinanceNet = Binance.Net;

namespace Infrastructure.Exchanges.Binance.Services;

public class BinanceService : Core.Interfaces.IExchangeService
{
	private readonly IBinanceClientFactory _clientFactory;
	private readonly IMapper _mapper;

	public BinanceService(IBinanceClientFactory clientFactory, IMapper mapper)
	{
		_clientFactory = clientFactory;
		_mapper = mapper;
	}

	/// <summary>
	/// Tests connectivity to the Binance API.
	/// </summary>
	/// <returns>
	/// True if connection is successful, otherwise false.
	/// </returns>
	public async Task<bool> TestConnectivity(ExchangeConnection connection, CancellationToken cancellationToken = default)
	{
		var client = _clientFactory.CreateClient(connection);
		var result = await client.SpotApi.ExchangeData.PingAsync(cancellationToken);
		return result.Success;
	}

	/// <summary>
	/// Retrieves the current server time from Binance.
	/// </summary>
	/// <param name="cancellationToken">Token used to cancel the operation.</param>
	/// <returns>Server time as Unix timestamp in milliseconds.</returns>
	/// <exception cref="InvalidOperationException">Thrown when the Binance API fails to return server time.</exception>
	public async Task<long> GetServerTime(ExchangeConnection connection, CancellationToken cancellationToken = default)
	{
		var client = _clientFactory.CreateClient(connection);
		var result = await client.SpotApi.ExchangeData.GetServerTimeAsync(cancellationToken);

		if (!result.Success)
			throw new InvalidOperationException($"Failed to retrieve server time from Binance: {result.Error?.Message}");

		return new DateTimeOffset(result.Data).ToUnixTimeMilliseconds();
	}

	/// <summary>
	/// Retrieves trading pair information including price and quantity constraints from Binance.
	/// </summary>
	/// <param name="symbol">Trading pair symbol (e.g. "BTCUSDT").</param>
	/// <param name="cancellationToken">Token used to cancel the operation.</param>
	/// <returns>
	/// Trading pair details with filters and constraints,
	/// or <see langword="null"/> if the symbol does not exist on Binance.
	/// </returns>
	/// <exception cref="InvalidOperationException">Thrown when the Binance API returns an unexpected error.</exception>
	/// <exception cref="TradingPairNotFoundException">Thrown when the specified symbol does not exist on Binance.</exception>
	public async Task<TradingPair?> GetTradingPairInfo(string symbol, ExchangeConnection connection, CancellationToken cancellationToken = default)
	{
		var client = _clientFactory.CreateClient(connection);
		var result = await client.SpotApi.ExchangeData.GetExchangeInfoAsync(symbol, ct: cancellationToken);

		if (!result.Success)
			throw new InvalidOperationException($"Failed to retrieve exchange info for '{symbol}': {result.Error?.Message}");

		if (result.Data.Symbols.Length == 0)
			throw new TradingPairNotFoundException($"Trading pair '{symbol}' not found on Binance.");

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
	/// Retrieves the current ticker price for the specified trading pair from Binance.
	/// </summary>
	/// <param name="symbol">Trading pair symbol (e.g. "BTCUSDT").</param>
	/// <param name="cancellationToken">Token used to cancel the operation.</param>
	/// <returns>Current price data including last, bid, and ask prices.</returns>
	/// <exception cref="InvalidOperationException">Thrown when the Binance API fails to return ticker data.</exception>
	public async Task<Price> GetPrice(string symbol, ExchangeConnection connection, CancellationToken cancellationToken = default)
	{
		var client = _clientFactory.CreateClient(connection);
		var result = await client.SpotApi.ExchangeData.GetTickerAsync(symbol, cancellationToken);

		if (!result.Success)
			throw new InvalidOperationException($"Failed to retrieve price for '{symbol}': {result.Error?.Message}");

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
	/// Retrieves the user's portfolio including all non-zero asset balances from Binance.
	/// </summary>
	/// <param name="userId">Internal user identifier to associate with the returned portfolio.</param>
	/// <param name="cancellationToken">Token used to cancel the operation.</param>
	/// <returns>Portfolio with all non-zero balances at the time of the request.</returns>
	/// <exception cref="InvalidOperationException">Thrown when the Binance API fails to return account info.</exception>
	public async Task<Portfolio> GetPortfolio(string userId, ExchangeConnection connection, CancellationToken cancellationToken = default)
	{
		var client = _clientFactory.CreateClient(connection);
		var result = await client.SpotApi.Account.GetAccountInfoAsync(ct: cancellationToken);

		if (!result.Success)
			throw new InvalidOperationException($"Failed to retrieve account info from Binance: {result.Error?.Message}");

		return new Portfolio
		{
			UserId = userId,
			Exchange = ExchangeName.Binance,
			IsTestnet = connection.IsTestnet,
			UpdatedAt = DateTimeOffset.UtcNow,
			Balances = [.. result.Data.Balances
			.Where(b => b.Available > 0 || b.Locked > 0)
			.Select(b => new Balance
			{
				Asset = b.Asset,
				Free = b.Available,
				Locked = b.Locked
			})]
		};
	}

	/// <summary>
	/// Places a new <see cref="Order"/> on Binance after validating quantity, price, and notional value.
	/// </summary>
	/// <param name="order">Order details to place.</param>
	/// <param name="cancellationToken">Token used to cancel the operation.</param>
	/// <returns>Placed order with exchange-assigned ID and mapped domain fields.</returns>
	/// <exception cref="InvalidOperationException">
	/// Thrown when the trading pair is not found, not supported, or Binance rejects the order.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// Thrown when order quantity, price, or notional value fails validation.
	/// </exception>
	/// <exception cref="TradingPairNotFoundException">Thrown when the specified symbol does not exist on Binance.</exception>
	public async Task<Order> PlaceOrder(Order order, ExchangeConnection connection, CancellationToken cancellationToken = default)
	{
		var client = _clientFactory.CreateClient(connection);
		var binanceSymbol = await GetBinanceSymbol(order.Symbol, client, cancellationToken);

		if (!BinanceHelpers.ValidateQuantity(order.Quantity, binanceSymbol))
			throw new ArgumentException(
				$"Invalid quantity {order.Quantity} for symbol '{order.Symbol}'. " +
				$"Min: {BinanceHelpers.GetMinQuantity(binanceSymbol)}, " +
				$"Max: {BinanceHelpers.GetMaxQuantity(binanceSymbol)}, " +
				$"Step: {BinanceHelpers.GetStepSize(binanceSymbol)}",
				nameof(order));

		if (order.Type == OrderType.Limit)
		{
			if (!order.Price.HasValue)
				throw new ArgumentException("Price has no value. Price is required.", nameof(order));

			if (!BinanceHelpers.ValidatePrice(order.Price.Value, binanceSymbol))
				throw new ArgumentException(
					$"Invalid price {order.Price.Value} for symbol '{order.Symbol}'. " +
					$"Min: {BinanceHelpers.GetMinPrice(binanceSymbol)}, " +
					$"Max: {BinanceHelpers.GetMaxPrice(binanceSymbol)}, " +
					$"Tick: {BinanceHelpers.GetTickSize(binanceSymbol)}",
					nameof(order));

			if (!BinanceHelpers.ValidateNotional(order.Price.Value, order.Quantity, binanceSymbol, false))
			{
				var minNotional = BinanceHelpers.GetMinNotional(binanceSymbol);
				throw new ArgumentException(
					$"Order notional value ({order.Price.Value * order.Quantity:F8}) does not meet minimum requirements ({minNotional}) for '{order.Symbol}'.",
					nameof(order));
			}
		}

		var binanceOrderSide = BinanceHelpers.ToBinanceSide(order.Side);
		var binanceOrderType = BinanceHelpers.ToBinanceOrderType(order.Type);

		var result = await client.SpotApi.Trading.PlaceOrderAsync(
			symbol: order.Symbol,
			side: binanceOrderSide,
			type: binanceOrderType,
			quantity: order.Quantity,
			price: order.Price,
			timeInForce: binanceOrderType == BinanceNet.Enums.SpotOrderType.Limit ? BinanceNet.Enums.TimeInForce.GoodTillCanceled : null,
			ct: cancellationToken
		);

		if (!result.Success)
			throw new InvalidOperationException($"Binance rejected the order: {result.Error?.Message}");

		var mappedOrder = _mapper.Map<Order>(result.Data);
		mappedOrder.UserId = order.UserId;
		mappedOrder.IsTestnet = connection.IsTestnet;
		return mappedOrder;
	}

	/// <summary>
	/// Retrieves details of a specific order from Binance.
	/// </summary>
	/// <param name="symbol">Trading pair symbol (e.g. "BTCUSDT").</param>
	/// <param name="orderId">Exchange-assigned order ID.</param>
	/// <param name="cancellationToken">Token used to cancel the operation.</param>
	/// <returns>Order details</returns>
	/// <exception cref="InvalidOperationException">Thrown when the Binance API returns an unexpected error.</exception>
	public async Task<Order> GetOrder(string symbol, long orderId, ExchangeConnection connection, CancellationToken cancellationToken = default)
	{
		var client = _clientFactory.CreateClient(connection);
		var result = await client.SpotApi.Trading.GetOrderAsync(symbol, orderId, ct: cancellationToken);

		if (!result.Success)
			throw new InvalidOperationException($"Failed to retrieve order {orderId}: {result.Error?.Message}");

		var mappedOrder = _mapper.Map<Order>(result.Data);
		mappedOrder.IsTestnet = connection.IsTestnet;
		return mappedOrder;
	}

	/// <summary>
	/// Cancels an active order on Binance.
	/// </summary>
	/// <param name="symbol">Trading pair symbol (e.g. "BTCUSDT").</param>
	/// <param name="orderId">Exchange-assigned order ID to cancel.</param>
	/// <param name="cancellationToken">Token used to cancel the operation.</param>
	/// <returns>Cancelled order with updated state from Binance.</returns>
	/// <exception cref="InvalidOperationException">Thrown when the Binance API fails to cancel the order.</exception>
	public async Task<Order> CancelOrder(string symbol, long orderId, ExchangeConnection connection, CancellationToken cancellationToken = default)
	{
		var client = _clientFactory.CreateClient(connection);
		var result = await client.SpotApi.Trading.CancelOrderAsync(symbol, orderId, ct: cancellationToken);

		if (!result.Success)
			throw new InvalidOperationException($"Failed to cancel order {orderId}: {result.Error?.Message}");

		var mappedOrder = _mapper.Map<Order>(result.Data);
		mappedOrder.IsTestnet = connection.IsTestnet;
		return mappedOrder;
	}

	/// <summary>
	/// Retrieves all open orders for the specified symbol, or for all symbols if none is provided.
	/// </summary>
	/// <param name="symbol">Trading pair symbol (e.g. "BTCUSDT"), or <see langword="null"/> to retrieve open orders for all symbols.</param>
	/// <param name="cancellationToken">Token used to cancel the operation.</param>
	/// <returns>List of open orders, or an empty list if no open orders exist.</returns>
	/// <exception cref="InvalidOperationException">Thrown when the Binance API fails to return open orders.</exception>
	public async Task<List<Order>> GetOpenOrders(string? symbol, ExchangeConnection connection, CancellationToken cancellationToken = default)
	{
		var client = _clientFactory.CreateClient(connection);
		var result = await client.SpotApi.Trading.GetOpenOrdersAsync(symbol, ct: cancellationToken);

		if (!result.Success)
			throw new InvalidOperationException($"Failed to retrieve open orders: {result.Error?.Message}");

		return [.. result.Data.Select(o =>
	{
		var mappedOrder = _mapper.Map<Order>(o);
		mappedOrder.IsTestnet = connection.IsTestnet;
		return mappedOrder;
	})];
	}

	/// <summary>
	/// Retrieves trade history for the specified symbol with optional filters.
	/// </summary>
	/// <param name="symbol">Trading pair symbol (e.g. "BTCUSDT").</param>
	/// <param name="userId">Internal user identifier to associate with each returned trade.</param>
	/// <param name="orderId">Filter trades by a specific exchange-assigned order ID.</param>
	/// <param name="startTime">Filter trades executed at or after this time.</param>
	/// <param name="endTime">Filter trades executed at or before this time.</param>
	/// <param name="limit">Maximum number of trades to return (default: 500).</param>
	/// <param name="cancellationToken">Token used to cancel the operation.</param>
	/// <returns>List of trades matching the specified filters, or an empty list if none exist.</returns>
	/// <exception cref="InvalidOperationException">Thrown when the Binance API fails to return trade history.</exception>
	public async Task<List<Trade>> GetTrades(string symbol, ExchangeConnection connection, string? userId = null, long? orderId = null, DateTime? startTime = null, DateTime? endTime = null, int limit = 500, CancellationToken cancellationToken = default)
	{
		var client = _clientFactory.CreateClient(connection);
		var result = await client.SpotApi.Trading.GetUserTradesAsync(
			symbol: symbol,
			orderId: orderId,
			startTime: startTime,
			endTime: endTime,
			limit: limit,
			ct: cancellationToken
		);

		if (!result.Success)
			throw new InvalidOperationException($"Failed to retrieve trades for '{symbol}': {result.Error?.Message}");

		return [.. result.Data.Select(t =>
	{
		var mappedTrade = _mapper.Map<Trade>(t);
		mappedTrade.UserId = userId;
		mappedTrade.IsTestnet = connection.IsTestnet;
		return mappedTrade;
	})];
	}

	/// <summary>
	/// Retrieves raw Binance symbol metadata used for order validation.
	/// </summary>
	/// <param name="symbol">Trading pair symbol (e.g. "BTCUSDT").</param>
	/// <param name="cancellationToken">Token used to cancel the operation.</param>
	/// <returns>Binance symbol metadata with filters and constraints.</returns>
	/// <exception cref="TradingPairNotFoundException">Thrown when the specified symbol does not exist on Binance.</exception>
	/// <exception cref="InvalidOperationException">Thrown when the Binance API returns an unexpected error.</exception>
	private async Task<BinanceSymbol> GetBinanceSymbol(string symbol, IBinanceRestClient client, CancellationToken cancellationToken)
	{
		var result = await client.SpotApi.ExchangeData.GetExchangeInfoAsync(symbol, ct: cancellationToken);

		if (!result.Success)
			throw new InvalidOperationException($"Failed to retrieve symbol info for '{symbol}': {result.Error?.Message}");

		if (result.Data.Symbols.Length == 0)
			throw new TradingPairNotFoundException($"Trading pair '{symbol}' not found on Binance.");

		return result.Data.Symbols[0];
	}
}
