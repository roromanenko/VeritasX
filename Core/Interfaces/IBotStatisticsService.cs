using Core.Domain.Statistics;

namespace Core.Interfaces;

public interface IBotStatisticsService
{
	Task<BotStatistics?> GetBotStatisticsAsync(string botId, string userId, DateOnly? from, DateOnly? to, CancellationToken ct = default);
	Task<AccountStatistics> GetAccountStatisticsAsync(string userId, DateOnly? from, DateOnly? to, CancellationToken ct = default);
}
