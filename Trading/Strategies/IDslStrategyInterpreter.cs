namespace Trading.Strategies;

/// <summary>
/// Builds an <see cref="ITradingStrategy"/> from a JSON snapshot string
/// and extracts <see cref="StrategyMetadata"/> describing the strategy's
/// data and timing requirements. This is the core extensibility point
/// for adding new strategy formats (builtin, DSL, visual graph).
/// </summary>
public interface IDslStrategyInterpreter
{
	/// <summary>
	/// Deserializes <paramref name="snapshot"/> and returns a ready-to-run strategy instance.
	/// </summary>
	ITradingStrategy Build(string snapshot);

	/// <summary>
	/// Extracts metadata (data requirement, tick interval) from <paramref name="snapshot"/>
	/// without building the full strategy.
	/// </summary>
	StrategyMetadata GetMetadata(string snapshot);
}

/// <summary>
/// Describes the data and timing requirements of a strategy,
/// extracted from a snapshot without constructing the full strategy.
/// </summary>
public class StrategyMetadata
{
	/// <summary>The type of market data the strategy needs (Ticker or Kline).</summary>
	public required DataRequirement DataRequirement { get; init; }

	/// <summary>Optional fixed interval between strategy ticks. Null means the strategy reacts to every incoming update.</summary>
	public TimeSpan? Interval { get; init; }
}
