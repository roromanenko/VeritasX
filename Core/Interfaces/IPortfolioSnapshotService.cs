using Core.Domain.Statistics;

namespace Core.Interfaces;

public interface IPortfolioSnapshotService
{
	Task<List<ExchangeSummary>> GetPortfolioSnapshotAsync(
		string userId,
		CancellationToken ct = default);
}
