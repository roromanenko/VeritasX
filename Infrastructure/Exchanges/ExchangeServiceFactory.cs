using Core.Domain;
using Core.Interfaces;
using Infrastructure.Exchanges.Binance.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Exchanges;

/// <summary>
/// Resolves the appropriate <see cref="IExchangeService"/> implementation
/// for the given <see cref="ExchangeName"/> from the DI container.
/// </summary>
public class ExchangeServiceFactory : IExchangeServiceFactory
{
	private readonly IServiceProvider _serviceProvider;

	public ExchangeServiceFactory(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

	/// <summary>
	/// Creates an <see cref="IExchangeService"/> instance for the specified exchange.
	/// </summary>
	/// <param name="exchange">The target exchange.</param>
	/// <returns>A resolved <see cref="IExchangeService"/> for the given exchange.</returns>
	/// <exception cref="NotSupportedException">Thrown when the exchange is not supported.</exception>
	public IExchangeService Create(ExchangeName exchange) => exchange switch
	{
		ExchangeName.Binance => _serviceProvider.GetRequiredService<BinanceService>(),
		_ => throw new NotSupportedException($"Exchange '{exchange}' is not supported.")
	};
}
