using Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
	public interface IExchangeService
	{
		// Connectivity
		Task<bool> TestConnectivity(CancellationToken cancellationToken = default);
		Task<long> GetServerTime(CancellationToken cancellationToken = default);

		// Market Data
		Task<TradingPair?> GetTradingPairInfo(string symbol, CancellationToken cancellationToken = default);
		Task<Price?> GetPrice(string symbol, CancellationToken cancellationToken = default);
		Task<List<Candle>> GetCandles(string symbol, string interval, DateTime? startTime = null, DateTime? endTime = null, int limit = 500, CancellationToken cancellationToken = default);

		// Account
		Task<Portfolio?> GetPortfolio(string userId, CancellationToken cancellationToken = default);

		// Orders
		Task<Order?> PlaceOrder(Order order, CancellationToken cancellationToken = default);
		Task<Order?> GetOrder(string symbol, long orderId, CancellationToken cancellationToken = default);
		Task<Order?> CancelOrder(string symbol, long orderId, CancellationToken cancellationToken = default);
		Task<List<Order>> GetOpenOrders(string? symbol = null, CancellationToken cancellationToken = default);

		// Trades
		Task<List<Trade>> GetTrades(string symbol, string? userId = null, long? orderId = null, DateTime? startTime = null, DateTime? endTime = null, int limit = 500, CancellationToken cancellationToken = default);
	}
}
