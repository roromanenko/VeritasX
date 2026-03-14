using Infrastructure.Persistence.Entities;
using MongoDB.Bson;

namespace Infrastructure.Interfaces;

public interface ITradeRepository
{
	Task<TradeDocument> CreateTrade(TradeDocument newTrade);
	Task<TradeDocument?> GetTradeById(ObjectId tradeId);
	Task<IEnumerable<TradeDocument>> GetTradeByUserId(ObjectId userId, int limit = 100);
	Task<IEnumerable<TradeDocument>> GetTradeBySymbol(ObjectId userId, string symbol, int limit = 100);
	Task<IEnumerable<TradeDocument>> GetTradeByDateRange(ObjectId userId, DateTimeOffset from, DateTimeOffset to);
	Task<IEnumerable<TradeDocument>> GetTradeByExchangeOrderId(ObjectId userId, string exchangeOrderId);
}
