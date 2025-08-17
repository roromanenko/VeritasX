namespace Api.DTO;

public record UserDto
(
	string Id,
	string Username,
	IEnumerable<string> Roles
);