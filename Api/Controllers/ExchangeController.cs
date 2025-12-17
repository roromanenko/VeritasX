using Api.DTO;
using AutoMapper;
using Core.Domain;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VeritasX.Api.Controllers;

namespace Api.Controllers
{
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
		public async Task<ActionResult<TradingPairDto>> GetTradingPair(string symbol, CancellationToken cancellationToken)
		{
			var tradingPair = await _exchangeService.GetTradingPairInfo(symbol, cancellationToken);

			if (tradingPair == null)
				return NotFound();

			var tradingPairDto = _mapper.Map<TradingPairDto>(tradingPair);
			return Ok(tradingPairDto);
		}

		/// <summary>
		/// Gets the current price for a trading pair.
		/// </summary>
		[HttpGet("price/{symbol}")]
		public async Task<ActionResult<PriceDto>> GetPrice(string symbol, CancellationToken cancellationToken)
		{
			var price = await _exchangeService.GetPrice(symbol, cancellationToken);

			if (price == null)
				return NotFound();

			var priceDto = _mapper.Map<PriceDto>(price);
			return Ok(priceDto);
		}

		/// <summary>
		/// Gets user portfolio with all asset balances.
		/// </summary>
		[HttpGet("portfolio")]
		[Authorize]
		public async Task<ActionResult<PortfolioDto>> GetPortfolio(CancellationToken cancellationToken)
		{
			var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name;

			if (string.IsNullOrEmpty(userId))
				return Unauthorized();

			var portfolio = await _exchangeService.GetPortfolio(userId, cancellationToken);

			if (portfolio == null)
				return NotFound();

			var portfolioDto = _mapper.Map<PortfolioDto>(portfolio);
			return Ok(portfolioDto);
		}

		/// <summary>
		/// Places a new order on the exchange.
		/// </summary>
		[HttpPost("orders")]
		[Authorize]
		public async Task<ActionResult<OrderDto>> PlaceOrder([FromBody] PlaceOrderRequest request, CancellationToken cancellationToken)
		{
			var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name;

			if (string.IsNullOrEmpty(userId))
				return Unauthorized();

			var order = _mapper.Map<Order>(request);
			order.UserId = userId;
			order.IsTestnet = true;

			var placedOrder = await _exchangeService.PlaceOrder(order, cancellationToken);

			if (placedOrder == null)
				return BadRequest("Failed to place order. Check validation errors.");

			var orderDto = _mapper.Map<OrderDto>(placedOrder);
			return CreatedAtAction(nameof(GetOrder), new { symbol = orderDto.Symbol, orderId = orderDto.ExchangeOrderId }, orderDto);
		}

		/// <summary>
		/// Gets details of a specific order.
		/// </summary>
		[HttpGet("orders/{symbol}/{orderId}")]
		[Authorize]
		public async Task<ActionResult<OrderDto>> GetOrder(string symbol, long orderId, CancellationToken cancellationToken)
		{
			var order = await _exchangeService.GetOrder(symbol, orderId, cancellationToken);

			if (order == null)
				return NotFound();

			var orderDto = _mapper.Map<OrderDto>(order);
			return Ok(orderDto);
		}

		/// <summary>
		/// Cancels an open order.
		/// </summary>
		[HttpDelete("orders/{symbol}/{orderId}")]
		[Authorize]
		public async Task<ActionResult<OrderDto>> CancelOrder(string symbol, long orderId, CancellationToken cancellationToken)
		{
			var cancelledOrder = await _exchangeService.CancelOrder(symbol, orderId, cancellationToken);

			if (cancelledOrder == null)
				return NotFound();

			var orderDto = _mapper.Map<OrderDto>(cancelledOrder);
			return Ok(orderDto);
		}

		/// <summary>
		/// Gets all open orders for a symbol or all symbols.
		/// </summary>
		[HttpGet("orders/open")]
		[Authorize]
		public async Task<ActionResult<OrdersResponse>> GetOpenOrders([FromQuery] string? symbol = null, CancellationToken cancellationToken = default)
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

		/// <summary>
		/// Gets trade history for a symbol.
		/// </summary>
		[HttpGet("trades/{symbol}")]
		[Authorize]
		public async Task<ActionResult<TradesResponse>> GetTrades(
			string symbol,
			[FromQuery] long? orderId = null,
			[FromQuery] DateTime? startTime = null,
			[FromQuery] DateTime? endTime = null,
			[FromQuery] int limit = 100,
			CancellationToken cancellationToken = default)
		{
			var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name;

			var trades = await _exchangeService.GetTrades(symbol, userId, orderId, startTime, endTime, limit, cancellationToken);
			var tradesDto = _mapper.Map<List<TradeDto>>(trades);

			return Ok(new TradesResponse
			{
				Symbol = symbol,
				Count = tradesDto.Count,
				Trades = tradesDto
			});
		}
	}
}
