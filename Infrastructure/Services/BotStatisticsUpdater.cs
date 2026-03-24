using System.Threading.Channels;
using Core.Domain.Statistics;
using Core.Interfaces;

namespace Infrastructure.Services;

public class BotStatisticsUpdater : IBotStatisticsUpdater
{
	private readonly Channel<MarketTickEvent> _tickChannel = Channel.CreateBounded<MarketTickEvent>(
		new BoundedChannelOptions(1000) { FullMode = BoundedChannelFullMode.DropOldest });

	private readonly Channel<TradeExecutedEvent> _tradeChannel = Channel.CreateUnbounded<TradeExecutedEvent>();

	public ChannelReader<MarketTickEvent> TickReader => _tickChannel.Reader;
	public ChannelReader<TradeExecutedEvent> TradeReader => _tradeChannel.Reader;

	public void OnTick(MarketTickEvent evt) => _tickChannel.Writer.TryWrite(evt);
	public void OnTrade(TradeExecutedEvent evt) => _tradeChannel.Writer.TryWrite(evt);
}
