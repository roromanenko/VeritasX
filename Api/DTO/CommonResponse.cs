namespace Api.DTO;

public record ApiResponse<T>(bool Success, string Message, T? Data = default);