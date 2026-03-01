using Infrastructure.Persistence.Entities;
using MongoDB.Bson;

namespace Infrastructure.Interfaces;

public interface ITradeRepository
{
	Task<TradeEntity> CreateTrade(TradeEntity newTrade);
	Task<TradeEntity?> GetTradeById(ObjectId tradeId);
	Task<IEnumerable<TradeEntity>> GetTradeByUserId(ObjectId userId, int limit = 100);
	Task<IEnumerable<TradeEntity>> GetTradeBySymbol(ObjectId userId, string symbol, int limit = 100);
	Task<IEnumerable<TradeEntity>> GetTradeByDateRange(ObjectId userId, DateTimeOffset from, DateTimeOffset to);
	Task<IEnumerable<TradeEntity>> GetTradeByExchangeOrderId(ObjectId userId, string exchangeOrderId);
}
