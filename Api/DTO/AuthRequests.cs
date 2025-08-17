namespace Api.DTO;

public record LoginRequest(string Username, string Password);
public record RegisterRequest(string Username, string Password, string ConfirmPassword);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmPassword);
public record NewPasswordRequest(string NewPassword, string ConfirmPassword);