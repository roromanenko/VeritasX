using Core.Domain;
using Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Trading;

public class BotRunnerFactory : IBotRunnerFactory
{
	private readonly IServiceScopeFactory _scopeFactory;

	public BotRunnerFactory(IServiceScopeFactory scopeFactory)
	{
		_scopeFactory = scopeFactory;
	}

	public IBotRunner Create(BotConfiguration bot)
	{
		var scope = _scopeFactory.CreateScope();
		return ActivatorUtilities.CreateInstance<BotRunner>(scope.ServiceProvider, scope, bot);
	}
}
