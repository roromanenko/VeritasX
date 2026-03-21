namespace Core.Interfaces;

public interface IBotRunner : IAsyncDisposable
{
	string BotId { get; }
	Task StartAsync(CancellationToken ct);
	Task StopAsync();
}
