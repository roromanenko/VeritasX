using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Infrastructure.Persistence.Entities;

[Table("strategies")]
public class StrategyDocument
{
	[BsonId]
	public ObjectId Id { get; set; }
	public ObjectId AuthorId { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public bool IsVeritasOfficial { get; set; }
	public bool IsPublic { get; set; }
	[BsonIgnoreIfNull]
	public decimal? Price { get; set; }
	[BsonIgnoreIfNull]
	public string? DslText { get; set; }
	[BsonIgnoreIfNull]
	public string? GraphJson { get; set; }
	public int Version { get; set; } = 1;
	public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
	public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
