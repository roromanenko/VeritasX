using Binance.Net;
using Binance.Net.Clients;
using Binance.Net.Interfaces.Clients;
using Binance.Net.Objects.Options;
using Core.Domain;
using CryptoExchange.Net.Authentication;

namespace Infrastructure.Exchanges.Binance.Factory;

public class BinanceClientFactory : IBinanceClientFactory
{
	public IBinanceRestClient CreateClient(ExchangeConnection connection)
	{
		var credentials = new ApiCredentials(connection.ApiKey, connection.SecretKey);

		var options = new BinanceRestOptions
		{
			ApiCredentials = credentials
		};

		if (connection.IsTestnet)
			options.Environment = BinanceEnvironment.Testnet;

		return new BinanceRestClient(opt =>
		{
			opt.ApiCredentials = credentials;
			opt.Environment = connection.IsTestnet
				? BinanceEnvironment.Testnet
				: BinanceEnvironment.Live;
		});
	}
}
