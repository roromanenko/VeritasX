using Core.Domain;

namespace Core.Interfaces;

public interface IExchangeService
{
	// Connectivity
	Task<bool> TestConnectivity(ExchangeConnection connection, CancellationToken cancellationToken = default);
	Task<long> GetServerTime(ExchangeConnection connection, CancellationToken cancellationToken = default);

	// Market Data
	Task<TradingPair?> GetTradingPairInfo(string symbol, ExchangeConnection connection, CancellationToken cancellationToken = default);
	Task<Price> GetPrice(string symbol, ExchangeConnection connection, CancellationToken cancellationToken = default);

	// Account
	Task<Portfolio> GetPortfolio(string userId, ExchangeConnection connection, CancellationToken cancellationToken = default);

	// Orders
	Task<Order> PlaceOrder(Order order, ExchangeConnection connection, CancellationToken cancellationToken = default);
	Task<Order> GetOrder(string symbol, long orderId, ExchangeConnection connection, CancellationToken cancellationToken = default);
	Task<Order> CancelOrder(string symbol, long orderId, ExchangeConnection connection, CancellationToken cancellationToken = default);
	Task<List<Order>> GetOpenOrders(string? symbol, ExchangeConnection connection, CancellationToken cancellationToken = default);

	// Trades
	Task<List<Trade>> GetTrades(string symbol,
		ExchangeConnection connection,
		string? userId = null,
		long? orderId = null,
		DateTime? startTime = null, 
		DateTime? endTime = null, 
		int limit = 500, 
		CancellationToken cancellationToken = default);
}
