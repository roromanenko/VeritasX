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
	public required string StrategyId { get; init; }
	public required string StrategySnapshot { get; set; }
	public int StrategyVersion { get; set; }
	public Dictionary<string, string> ParameterOverrides { get; set; } = new();
	public required RiskParameters RiskParameters { get; set; }
	public BotStatus Status { get; set; } = BotStatus.Stopped;
	public string? ErrorMessage { get; set; }
	/// <summary>
	/// Number of consecutive tick errors before the bot self-stops. Default: 5.
	/// Minimum of 1 prevents misconfiguration where a bot never stops on errors.
	/// </summary>
	public int MaxConsecutiveErrors { get; set; } = 5;
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

public class RiskParameters
{
	public decimal PositionSize { get; set; }
	public decimal? StopLoss { get; set; }
	public decimal? TakeProfit { get; set; }
}
