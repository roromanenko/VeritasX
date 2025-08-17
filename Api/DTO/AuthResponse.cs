namespace Api.DTO;

public record LoginResponse(UserDto User, string AccessToken);