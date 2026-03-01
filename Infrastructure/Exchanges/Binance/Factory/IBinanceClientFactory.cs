using Binance.Net.Interfaces.Clients;
using Core.Domain;

namespace Infrastructure.Exchanges.Binance.Factory;

public interface IBinanceClientFactory
{
	IBinanceRestClient CreateClient(ExchangeConnection connection);
}
