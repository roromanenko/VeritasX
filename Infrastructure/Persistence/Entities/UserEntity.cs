using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Infrastructure.Persistence.Entities;

public class UserEntity
{
	[BsonId]
	public ObjectId Id { get; set; }

	[BsonRequired]
	public string Username { get; set; } = string.Empty;

	[BsonRequired]
	public string PasswordHash { get; set; } = string.Empty;
	public List<string> Roles { get; set; } = [];
}