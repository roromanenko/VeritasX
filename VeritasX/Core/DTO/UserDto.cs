namespace VeritasX.Core.DTO;

// For authorization
public record LoginRequest(string Username, string Password);

// For registration
public record RegisterRequest(string Username, string Password, string ConfirmPassword);

// For changing password
public record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmPassword);

// For setting a new password (reset password)
public record NewPasswordRequest(string NewPassword, string ConfirmPassword);

// Response with user data
public record UserResponse(string Id, string Username, List<string> Roles);

// Successful authorization response
public record LoginResponse(UserResponse User, string Token);

// Base API response
public record ApiResponse<T>(bool Success, string Message, T? Data = default);

// Error response
public record ErrorResponse(string Message, Dictionary<string, string[]>? Errors = null); 