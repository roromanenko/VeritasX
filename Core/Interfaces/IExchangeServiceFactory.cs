using Core.Domain;

namespace Core.Interfaces;

public interface IExchangeServiceFactory
{
	IExchangeService Create(ExchangeName exchange);
}
