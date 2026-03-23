namespace Core.Domain;

/// <summary>
/// Represents a trading strategy entry in the marketplace.
/// Strategies can be official (built by VeritasX) or user-created,
/// and may be public or private.
/// </summary>
public class Strategy
{
	public required string Id { get; init; }
	public required string AuthorId { get; init; }
	public required string Name { get; set; }
	public string Description { get; set; } = string.Empty;
	public bool IsVeritasOfficial { get; set; }
	public bool IsPublic { get; set; }
	public decimal? Price { get; set; }
	public string? DslText { get; set; }
	public string? GraphJson { get; set; }
	public int Version { get; set; } = 1;
	public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
	public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
