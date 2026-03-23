using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Infrastructure.Persistence.Entities;

[Table("user_strategy_library")]
public class UserStrategyLibraryDocument
{
	[BsonId]
	public ObjectId Id { get; set; }
	public ObjectId UserId { get; set; }
	public ObjectId StrategyId { get; set; }
	public string SavedDslSnapshot { get; set; } = string.Empty;
	[BsonIgnoreIfNull]
	public string? SavedGraphSnapshot { get; set; }
	public int SavedVersion { get; set; }
	public Dictionary<string, string> ParameterOverrides { get; set; } = new();
	public DateTimeOffset SavedAt { get; set; } = DateTimeOffset.UtcNow;
}
