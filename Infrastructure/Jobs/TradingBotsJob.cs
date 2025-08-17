using Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Trading;
using Trading.Processors;
using Trading.Strategies;

namespace Infrastructure.Jobs;

public class TradingBotsJob : BackgroundService
{
	private readonly IServiceProvider _serviceProvider;

	public TradingBotsJob(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		using var scope = _serviceProvider.CreateScope();

		ITradingProcessor processor = new TestTradingProcessor(
			scope.ServiceProvider.GetRequiredService<ICandleChunkService>(),
			"68a1f2b636456817db50afd2",
			"USD",
			"BTC",
			1000m);

		var context = new TradingContext(new AccountContext("tess"), new TestPriceProvider());

		await processor.Start(new RebalanceToTargetStrategy(),
			"""
			{
				"Asset": "BTC",
				"Threshold": 0.02
			}
			""",
			context,
			stoppingToken);

	}

	public override Task StopAsync(CancellationToken cancellationToken)
	{
		return base.StopAsync(cancellationToken);
	}
}
