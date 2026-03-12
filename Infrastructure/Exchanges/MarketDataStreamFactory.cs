using Core.Domain;
using Core.Interfaces;
using Infrastructure.Exchanges.Binance.Streams;

namespace Infrastructure.Exchanges;

public class MarketDataStreamFactory : IMarketDataStreamFactory
{
	public IMarketDataStream Create(ExchangeName exchange, ExchangeConnection connection)
		=> exchange switch
		{
			ExchangeName.Binance => new BinanceMarketDataStream(connection),
			_ => throw new NotSupportedException($"Exchange '{exchange}' is not supported.")
		};
}
