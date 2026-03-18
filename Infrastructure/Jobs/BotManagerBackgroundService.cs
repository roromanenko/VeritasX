using System.Collections.Concurrent;
using System.Text;
using AutoMapper;
using Core.Domain;
using Core.Interfaces;
using Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Jobs;

public class BotManagerBackgroundService : BackgroundService
{
	private readonly IBotRunnerFactory _botRunnerFactory;
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly ILogger<BotManagerBackgroundService> _logger;

	private readonly ConcurrentDictionary<string, (IBotRunner Runner, Task Task, CancellationTokenSource Cts)> _runners = new();

	public BotManagerBackgroundService(
		IBotRunnerFactory botRunnerFactory,
		IServiceScopeFactory scopeFactory,
		ILogger<BotManagerBackgroundService> logger)
	{
		_botRunnerFactory = botRunnerFactory;
		_scopeFactory = scopeFactory;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("BotManagerBackgroundService started.");

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await StartPendingBotsAsync(stoppingToken);
				await StopRemovedBotsAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "BotManagerBackgroundService error on poll cycle.");
			}

			await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
		}

		await StopAllRunnersAsync();
	}

	private async Task StartPendingBotsAsync(CancellationToken stoppingToken)
	{
		using var scope = _scopeFactory.CreateScope();
		var botRepository = scope.ServiceProvider.GetRequiredService<IBotRepository>();
		var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

		var pendingBots = await botRepository.GetPendingBots();

		foreach (var botDocument in pendingBots)
		{
			if (_runners.ContainsKey(botDocument.Id.ToString()))
				continue;

			var bot = mapper.Map<BotConfiguration>(botDocument);
			var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
			var runner = _botRunnerFactory.Create(bot);
			var task = Task.Run(async () =>
			{
				try
				{
					await runner.StartAsync(cts.Token);
				}
				finally
				{
					if (runner is IAsyncDisposable disposable)
						await disposable.DisposeAsync();
				}
			});

			_runners[bot.Id] = (runner, task, cts);

			_logger.LogInformation("Bot {BotId} picked up and started.", bot.Id);
		}
	}

	private async Task StopRemovedBotsAsync()
	{
		using var scope = _scopeFactory.CreateScope();
		var botRepository = scope.ServiceProvider.GetRequiredService<IBotRepository>();

		var activeBots = await botRepository.GetActiveBots();
		var pendingBots = await botRepository.GetPendingBots();

		var runningIds = activeBots.Select(b => b.Id.ToString())
			.Concat(pendingBots.Select(b => b.Id.ToString()))
			.ToHashSet();

		foreach (var (botId, (_, _, cts)) in _runners)
		{
			if (!runningIds.Contains(botId))
			{
				_logger.LogInformation("Bot {BotId} is no longer active, stopping runner.", botId);
				await cts.CancelAsync();
				_runners.TryRemove(botId, out _);
			}
		}

		foreach (var (botId, (_, task, _)) in _runners)
		{
			if (task.IsCompleted)
				_runners.TryRemove(botId, out _);
		}
	}

	private async Task StopAllRunnersAsync()
	{
		_logger.LogInformation("BotManagerBackgroundService stopping all runners.");

		var stopTasks = _runners.Values.Select(async entry =>
		{
			await entry.Cts.CancelAsync();
			await entry.Task;
		});

		await Task.WhenAll(stopTasks);
		_runners.Clear();
	}
}
