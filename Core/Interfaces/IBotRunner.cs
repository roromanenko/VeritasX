namespace Core.Interfaces;

public interface IBotRunner
{
	string BotId { get; }
	Task StartAsync(CancellationToken ct);
	Task StopAsync();
}
