namespace Core.Domain;

/// <summary>
/// Represents a user's saved copy of a strategy from the marketplace.
/// Contains the DSL/graph snapshot at the time of saving, along with
/// any user-specific parameter overrides.
/// </summary>
public class UserStrategyLibrary
{
	public required string Id { get; init; }
	public required string UserId { get; init; }
	public required string StrategyId { get; init; }
	public required string SavedDslSnapshot { get; set; }
	public string? SavedGraphSnapshot { get; set; }
	public int SavedVersion { get; set; }
	public Dictionary<string, string> ParameterOverrides { get; set; } = new();
	public DateTimeOffset SavedAt { get; set; } = DateTimeOffset.UtcNow;
}
