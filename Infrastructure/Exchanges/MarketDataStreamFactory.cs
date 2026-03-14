using Core.Domain;
using Core.Interfaces;
using Infrastructure.Exchanges.Binance.Streams;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Exchanges;

public class MarketDataStreamFactory : IMarketDataStreamFactory
{
	private readonly ILogger<BinanceMarketDataStream> _logger;

	public MarketDataStreamFactory(ILogger<BinanceMarketDataStream> logger)
	{
		_logger = logger;
	}

	public IMarketDataStream Create(ExchangeName exchange, ExchangeConnection connection)
		=> exchange switch
		{
			ExchangeName.Binance => new BinanceMarketDataStream(connection, _logger),
			_ => throw new NotSupportedException($"Exchange '{exchange}' is not supported.")
		};
}
