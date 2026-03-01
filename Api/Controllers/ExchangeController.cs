using Api.DTO;
using AutoMapper;
using Core.Domain;
using Core.Exceptions;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VeritasX.Api.Controllers;

namespace Api.Controllers;

[ApiController]
[Route("api/exchange/{exchange}")]
[Authorize]
public class ExchangeController : BaseController
{
	private readonly IExchangeService _exchangeService;
	private readonly IUserService _userService;
	private readonly IMapper _mapper;
	private readonly ILogger<ExchangeController> _logger;

	public ExchangeController(
		IExchangeService exchangeService,
		IUserService userService,
		IMapper mapper,
		ILogger<ExchangeController> logger)
	{
		_exchangeService = exchangeService;
		_userService = userService;
		_mapper = mapper;
		_logger = logger;
	}

	/// <summary>
	/// Tests connectivity to the exchange API.
	/// </summary>
	[HttpGet("connectivity")]
	[ProducesResponseType(typeof(ConnectivityResponse), StatusCodes.Status200OK)]
	public async Task<ActionResult<ConnectivityResponse>> TestConnectivity(ExchangeName exchange, CancellationToken cancellationToken)
	{
		var connection = await _userService.GetExchangeConnection(UserId!, exchange);
		var isConnected = await _exchangeService.TestConnectivity(connection, cancellationToken);
		return Ok(new ConnectivityResponse
		{
			IsConnected = isConnected,
			Timestamp = DateTimeOffset.UtcNow
		});
	}

	/// <summary>
	/// Gets the current server time from the exchange.
	/// </summary>
	[HttpGet("server-time")]
	[ProducesResponseType(typeof(ServerTimeResponse), StatusCodes.Status200OK)]
	public async Task<ActionResult<ServerTimeResponse>> GetServerTime(ExchangeName exchange, CancellationToken cancellationToken)
	{
		var connection = await _userService.GetExchangeConnection(UserId!, exchange);
		var serverTime = await _exchangeService.GetServerTime(connection, cancellationToken);
		return Ok(new ServerTimeResponse
		{
			ServerTime = serverTime,
			ServerDateTime = DateTimeOffset.FromUnixTimeMilliseconds(serverTime)
		});
	}

	/// <summary>
	/// Gets trading pair information including price and quantity constraints.
	/// </summary>
	[HttpGet("pairs/{symbol}")]
	[ProducesResponseType(typeof(TradingPairDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<TradingPairDto>> GetTradingPair(ExchangeName exchange, string symbol, CancellationToken cancellationToken)
	{
		try
		{
			var connection = await _userService.GetExchangeConnection(UserId!, exchange);
			var tradingPair = await _exchangeService.GetTradingPairInfo(symbol, connection, cancellationToken);
			return Ok(_mapper.Map<TradingPairDto>(tradingPair));
		}
		catch (TradingPairNotFoundException)
		{
			return NotFound(new { message = $"Trading pair '{symbol}' not found." });
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogError(ex, "Failed to retrieve trading pair info for {Symbol}", symbol);
			return StatusCode(StatusCodes.Status500InternalServerError,
				new { message = $"Exchange service is temporarily unavailable. {ex.Message}" });
		}
	}

	/// <summary>
	/// Gets the current price for a trading pair.
	/// </summary>
	[HttpGet("price/{symbol}")]
	[ProducesResponseType(typeof(PriceDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<PriceDto>> GetPrice(ExchangeName exchange, string symbol, CancellationToken cancellationToken)
	{
		try
		{
			var connection = await _userService.GetExchangeConnection(UserId!, exchange);
			var price = await _exchangeService.GetPrice(symbol, connection, cancellationToken);
			return Ok(_mapper.Map<PriceDto>(price));
		}
		catch (TradingPairNotFoundException ex)
		{
			return NotFound(new { message = ex.Message });
		}
	}

	/// <summary>
	/// Gets user portfolio with all asset balances.
	/// </summary>
	[HttpGet("portfolio")]
	[ProducesResponseType(typeof(PortfolioDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
	public async Task<ActionResult<PortfolioDto>> GetPortfolio(ExchangeName exchange, CancellationToken cancellationToken)
	{
		try
		{
			var connection = await _userService.GetExchangeConnection(UserId!, exchange);
			var portfolio = await _exchangeService.GetPortfolio(UserId!, connection, cancellationToken);
			return Ok(_mapper.Map<PortfolioDto>(portfolio));
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogError(ex, "Exchange API error while retrieving portfolio for user {UserId}", UserId);
			return StatusCode(StatusCodes.Status503ServiceUnavailable,
				new { message = $"Exchange service is temporarily unavailable. {ex.Message}" });
		}
	}

	/// <summary>
	/// Places a new order on the exchange.
	/// </summary>
	[HttpPost("orders")]
	[ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
	public async Task<ActionResult<OrderDto>> PlaceOrder(ExchangeName exchange, [FromBody] PlaceOrderRequest request, CancellationToken cancellationToken)
	{
		try
		{
			var connection = await _userService.GetExchangeConnection(UserId!, exchange);
			var order = _mapper.Map<Order>(request);
			order.UserId = UserId!;

			var placedOrder = await _exchangeService.PlaceOrder(order, connection, cancellationToken);
			var orderDto = _mapper.Map<OrderDto>(placedOrder);

			return CreatedAtAction(
				nameof(GetOrder),
				new { exchange, symbol = orderDto.Symbol, orderId = orderDto.ExchangeOrderId },
				orderDto);
		}
		catch (TradingPairNotFoundException ex)
		{
			_logger.LogWarning(ex, "Trading pair not found while placing order for user {UserId}", UserId);
			return NotFound(new { message = ex.Message });
		}
		catch (ArgumentException ex)
		{
			_logger.LogWarning(ex, "Order validation failed for user {UserId}: {Message}", UserId, ex.Message);
			return BadRequest(new { message = ex.Message });
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogError(ex, "Exchange API error while placing order for user {UserId}", UserId);
			return StatusCode(StatusCodes.Status503ServiceUnavailable,
				new { message = $"Exchange service is temporarily unavailable. {ex.Message}" });
		}
	}

	/// <summary>
	/// Gets details of a specific order.
	/// </summary>
	[HttpGet("orders/{symbol}/{orderId}")]
	[ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
	public async Task<ActionResult<OrderDto>> GetOrder(ExchangeName exchange, string symbol, long orderId, CancellationToken cancellationToken)
	{
		try
		{
			var connection = await _userService.GetExchangeConnection(UserId!, exchange);
			var order = await _exchangeService.GetOrder(symbol, orderId, connection, cancellationToken);
			return Ok(_mapper.Map<OrderDto>(order));
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogError(ex, "Exchange API error while retrieving order {OrderId} for symbol {Symbol}", orderId, symbol);
			return StatusCode(StatusCodes.Status503ServiceUnavailable,
				new { message = $"Exchange service is temporarily unavailable. {ex.Message}" });
		}
	}

	/// <summary>
	/// Cancels an open order.
	/// </summary>
	[HttpDelete("orders/{symbol}/{orderId}")]
	[ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<OrderDto>> CancelOrder(ExchangeName exchange, string symbol, long orderId, CancellationToken cancellationToken)
	{
		try
		{
			var connection = await _userService.GetExchangeConnection(UserId!, exchange);
			var cancelledOrder = await _exchangeService.CancelOrder(symbol, orderId, connection, cancellationToken);
			return Ok(_mapper.Map<OrderDto>(cancelledOrder));
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogWarning(ex, "Cannot cancel order {OrderId} for symbol {Symbol}", orderId, symbol);
			return BadRequest(new { message = ex.Message });
		}
	}

	/// <summary>
	/// Gets all open orders for a symbol or all symbols.
	/// </summary>
	[HttpGet("orders/open")]
	[ProducesResponseType(typeof(OrdersResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
	public async Task<ActionResult<OrdersResponse>> GetOpenOrders(ExchangeName exchange, [FromQuery] string? symbol, CancellationToken cancellationToken)
	{
		try
		{
			var connection = await _userService.GetExchangeConnection(UserId!, exchange);
			var orders = await _exchangeService.GetOpenOrders(symbol, connection, cancellationToken);
			var ordersDto = _mapper.Map<List<OrderDto>>(orders);
			return Ok(new OrdersResponse
			{
				Symbol = symbol ?? "all",
				Count = ordersDto.Count,
				Orders = ordersDto
			});
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogError(ex, "Exchange API error while retrieving open orders for user {UserId}", UserId);
			return StatusCode(StatusCodes.Status503ServiceUnavailable,
				new { message = $"Exchange service is temporarily unavailable. {ex.Message}" });
		}
	}

	/// <summary>
	/// Gets trade history for a symbol.
	/// </summary>
	[HttpGet("trades/{symbol}")]
	[ProducesResponseType(typeof(TradesResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
	public async Task<ActionResult<TradesResponse>> GetTrades(
		ExchangeName exchange,
		string symbol,
		[FromQuery] long? orderId = null,
		[FromQuery] DateTime? startTime = null,
		[FromQuery] DateTime? endTime = null,
		[FromQuery] int limit = 100,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var connection = await _userService.GetExchangeConnection(UserId!, exchange);
			var trades = await _exchangeService.GetTrades(symbol, connection, UserId!, orderId, startTime, endTime, limit, cancellationToken);
			var tradesDto = _mapper.Map<List<TradeDto>>(trades);
			return Ok(new TradesResponse
			{
				Symbol = symbol,
				Count = tradesDto.Count,
				Trades = tradesDto
			});
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogError(ex, "Exchange API error while retrieving trades for symbol {Symbol}", symbol);
			return StatusCode(StatusCodes.Status503ServiceUnavailable,
				new { message = $"Exchange service is temporarily unavailable. {ex.Message}" });
		}
	}
}
