using Core.Domain;

namespace Core.Interfaces;

public interface IMarketDataStreamFactory
{
	IMarketDataStream Create(ExchangeName exchange, ExchangeConnection connection);
}
