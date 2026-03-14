namespace Core.Domain;

public class BotConfiguration
{
	public required string Id { get; init; }
	public required string UserId { get; init; }
	public required string Name { get; set; }
	public required ExchangeName Exchange { get; init; }
	public required string Symbol { get; init; }
	public required string BaseAsset { get; init; }
	public required string QuoteAsset { get; init; }
	public required StrategyDefinition Strategy { get; init; }
	public required RiskParameters RiskParameters { get; set; }
	public BotStatus Status { get; set; } = BotStatus.Stopped;
	public string? ErrorMessage { get; set; }
	public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
	public DateTimeOffset? StartedAt { get; set; }
	public DateTimeOffset? StoppedAt { get; set; }
}

public enum BotStatus
{
	Active,
	Pending,
	Stopped,
	Error
}

public class StrategyDefinition
{
	public required StrategyType Type { get; init; }
	public required Dictionary<string, string> Parameters { get; init; }
}

public class RiskParameters
{
	public decimal PositionSize { get; set; }
	public decimal? StopLoss { get; set; }
	public decimal? TakeProfit { get; set; }
}
