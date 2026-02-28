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
[Route("api/[controller]")]
public class ExchangeController : BaseController
{
	private readonly IExchangeService _exchangeService;
	private readonly IMapper _mapper;
	private readonly ILogger<ExchangeController> _logger;

	public ExchangeController(
		IExchangeService exchangeService,
		IMapper mapper,
		ILogger<ExchangeController> logger)
	{
		_exchangeService = exchangeService;
		_mapper = mapper;
		_logger = logger;
	}

	/// <summary>
	/// Tests connectivity to the exchange API.
	/// </summary>
	[HttpGet("connectivity")]
	[ProducesResponseType(typeof(ConnectivityResponse), StatusCodes.Status200OK)]
	public async Task<ActionResult<ConnectivityResponse>> TestConnectivity(CancellationToken cancellationToken)
	{
		var isConnected = await _exchangeService.TestConnectivity(cancellationToken);
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
	public async Task<ActionResult<ServerTimeResponse>> GetServerTime(CancellationToken cancellationToken)
	{
		var serverTime = await _exchangeService.GetServerTime(cancellationToken);
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
	public async Task<ActionResult<TradingPairDto>> GetTradingPair(string symbol, CancellationToken cancellationToken)
	{
		try
		{
			var tradingPair = await _exchangeService.GetTradingPairInfo(symbol, cancellationToken);
			var tradingPairDto = _mapper.Map<TradingPairDto>(tradingPair);
			return Ok(tradingPairDto);
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
	public async Task<ActionResult<PriceDto>> GetPrice(string symbol, CancellationToken cancellationToken)
	{
		try
		{
			var price = await _exchangeService.GetPrice(symbol, cancellationToken);
			var priceDto = _mapper.Map<PriceDto>(price);
			return Ok(priceDto);
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
	[Authorize]
	[ProducesResponseType(typeof(PortfolioDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
	public async Task<ActionResult<PortfolioDto>> GetPortfolio(CancellationToken cancellationToken)
	{
		try
		{
			var portfolio = await _exchangeService.GetPortfolio(UserId!, cancellationToken);
			var portfolioDto = _mapper.Map<PortfolioDto>(portfolio);
			return Ok(portfolioDto);
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
	[Authorize]
	[ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
	public async Task<ActionResult<OrderDto>> PlaceOrder([FromBody] PlaceOrderRequest request, CancellationToken cancellationToken)
	{
		try
		{
			var order = _mapper.Map<Order>(request);
			order.UserId = UserId!;

			var placedOrder = await _exchangeService.PlaceOrder(order, cancellationToken);
			var orderDto = _mapper.Map<OrderDto>(placedOrder);

			return CreatedAtAction(
				nameof(GetOrder),
				new { symbol = orderDto.Symbol, orderId = orderDto.ExchangeOrderId },
				orderDto);
		}
		catch (TradingPairNotFoundException ex)
		{
			_logger.LogWarning(ex, $"Trading pair not found while placing order for user {UserId}");
			return NotFound(new { message = ex.Message });
		}
		catch (ArgumentException ex)
		{
			_logger.LogWarning(ex, $"Order validation failed for user {UserId}: {ex.Message}");
			return BadRequest(new { message = ex.Message });
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogError(ex, $"Exchange API error while placing order for user {UserId}");
			return StatusCode(StatusCodes.Status503ServiceUnavailable,
				new { message = $"Exchange service is temporarily unavailable. {ex.Message}" });
		}
	}

	/// <summary>
	/// Gets details of a specific order.
	/// </summary>
	[HttpGet("orders/{symbol}/{orderId}")]
	[Authorize]
	[ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
	public async Task<ActionResult<OrderDto>> GetOrder(string symbol, long orderId, CancellationToken cancellationToken)
	{
		try
		{
			var order = await _exchangeService.GetOrder(symbol, orderId, cancellationToken);
			var orderDto = _mapper.Map<OrderDto>(order);
			return Ok(orderDto);
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogError(ex, $"Exchange API error while retrieving order {orderId} for symbol {symbol}");
			return StatusCode(StatusCodes.Status503ServiceUnavailable,
				new { message = $"Exchange service is temporarily unavailable. {ex.Message}" });
		}
	}

	/// <summary>
	/// Cancels an open order.
	/// </summary>
	[HttpDelete("orders/{symbol}/{orderId}")]
	[Authorize]
	[ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<OrderDto>> CancelOrder(string symbol, long orderId, CancellationToken cancellationToken)
	{
		try
		{
			var cancelledOrder = await _exchangeService.CancelOrder(symbol, orderId, cancellationToken);
			var orderDto = _mapper.Map<OrderDto>(cancelledOrder);
			return Ok(orderDto);
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogWarning(ex, $"Cannot cancel order {orderId} for symbol {symbol}: {ex.Message}");
			return BadRequest(new { message = ex.Message });
		}
	}

	/// <summary>
	/// Gets all open orders for a symbol or all symbols.
	/// </summary>
	[HttpGet("orders/open")]
	[Authorize]
	[ProducesResponseType(typeof(OrdersResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
	public async Task<ActionResult<OrdersResponse>> GetOpenOrders([FromQuery] string? symbol = null, CancellationToken cancellationToken = default)
	{
		try
		{
			var orders = await _exchangeService.GetOpenOrders(symbol, cancellationToken);
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
			_logger.LogError(ex, $"Exchange API error while retrieving open orders for user {UserId}, symbol: {symbol}");
			return StatusCode(StatusCodes.Status503ServiceUnavailable,
				new { message = $"Exchange service is temporarily unavailable. {ex.Message}" });
		}
	}

	/// <summary>
	/// Gets trade history for a symbol.
	/// </summary>
	[HttpGet("trades/{symbol}")]
	[Authorize]
	[ProducesResponseType(typeof(TradesResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
	public async Task<ActionResult<TradesResponse>> GetTrades(
		string symbol,
		[FromQuery] long? orderId = null,
		[FromQuery] DateTime? startTime = null,
		[FromQuery] DateTime? endTime = null,
		[FromQuery] int limit = 100,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var trades = await _exchangeService.GetTrades(symbol, UserId!, orderId, startTime, endTime, limit, cancellationToken);
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
