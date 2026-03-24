using Core.Domain.Statistics;

namespace Core.Interfaces;

public interface IBotStatisticsUpdater
{
	void OnTick(MarketTickEvent evt);
	void OnTrade(TradeExecutedEvent evt);
}
