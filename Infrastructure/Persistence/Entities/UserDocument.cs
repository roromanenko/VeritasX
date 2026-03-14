using Core.Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Entities;

public class UserDocument
{
	[BsonId]
	public ObjectId Id { get; set; }

	[BsonRequired]
	public string Username { get; set; } = string.Empty;

	[BsonRequired]
	public string PasswordHash { get; set; } = string.Empty;
	public List<string> Roles { get; set; } = [];

	[BsonIgnoreIfNull]
	public Dictionary<ExchangeName, ExchangeConnectionDocument>? ExchangeConnections { get; set; }
}
