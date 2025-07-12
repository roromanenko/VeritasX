using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VeritasX.Infrastructure.Persistence.Entities;

[Table("user")]
public class User
{
	[BsonId]
	public ObjectId Id { get; set; }

	[Required]
	public string Username { get; set; } = string.Empty;

	[Required]
	public string PasswordHash { get; set; } = string.Empty;
	public List<string> Roles { get; set; } = new List<string>();
}