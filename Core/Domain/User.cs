namespace Core.Domain;

public sealed class User
{
	public required string Id { get; init; }
	public required string Username { get; set; }
	public required string PasswordHash { get; set; }
	public List<string> Roles { get; set; } = [];
}
