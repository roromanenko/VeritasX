using Core.Domain;
using Core.Interfaces;
using Infrastructure.Exchanges.Binance.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Exchanges;

public class ExchangeServiceFactory : IExchangeServiceFactory
{
	private readonly IServiceProvider _serviceProvider;

	public ExchangeServiceFactory(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

	public IExchangeService Create(ExchangeName exchange) => exchange switch
	{
		ExchangeName.Binance => _serviceProvider.GetRequiredService<BinanceService>(),
		_ => throw new NotSupportedException($"Exchange '{exchange}' is not supported.")
	};
}
