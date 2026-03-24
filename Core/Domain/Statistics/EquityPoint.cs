namespace Core.Domain.Statistics;

public class EquityPoint
{
	public DateOnly Date { get; init; }
	public decimal Equity { get; init; }
	public decimal DailyReturn { get; init; }
}
