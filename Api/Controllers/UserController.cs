using Microsoft.AspNetCore.Mvc;
using Infrastructure.Services;
using Core.Interfaces;
using Infrastructure.Persistence.Entities;
using VeritasX.Core.DTO;
using Microsoft.AspNetCore.Authorization;
using VeritasX.Api.Controllers;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : BaseController
{
	private readonly IUserService _userService;
	private readonly ILogger<UserController> _logger;
	private readonly IJwtService _jwtService;

	public UserController(IUserService userService, ILogger<UserController> logger, IJwtService jwtService)
	{
		_userService = userService;
		_logger = logger;
		_jwtService = jwtService;
	}

	[HttpPost("register")]
	public async Task<ActionResult<ApiResponse<UserResponse>>> Register(RegisterRequest request)
	{
		try
		{
			// Password validation
			if (request.Password != request.ConfirmPassword)
				return Ok(new ApiResponse<UserResponse>(false, "Passwords do not match"));

			var user = await _userService.RegisterUser(request.Username, request.Password);

			var response = new UserResponse(user.Id.ToString(), user.Username, user.Roles);
			return Ok(new ApiResponse<UserResponse>(true, "User registered successfully", response));
		}
		catch (ArgumentException ex)
		{
			return Ok(new ApiResponse<UserResponse>(false, ex.Message));
		}
		catch (Exception)
		{
			return Ok(new ApiResponse<UserResponse>(false, "Registration failed"));
		}
	}

	[HttpPost("login")]
	public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(LoginRequest request)
	{
		var user = await _userService.VerifyUserLogin(request.Username, request.Password);
		if (user == null)
			return Ok(new ApiResponse<LoginResponse>(false, "Invalid credentials"));

		var token = _jwtService.GenerateToken(user);

		var userResponse = new UserResponse(user.Id.ToString(), user.Username, user.Roles);
		var loginResponse = new LoginResponse(userResponse, token);

		return Ok(new ApiResponse<LoginResponse>(true, "Login successful", loginResponse));
	}

	[HttpPut("password")]
	[Authorize]
	public async Task<ActionResult<ApiResponse<string>>> ChangePassword([FromBody] ChangePasswordRequest request)
	{
		try
		{
			var user = await _userService.VerifyUserLogin(Username!, request.CurrentPassword);
			if (user == null)
				return Ok(new ApiResponse<string>(false, "Current password is incorrect"));

			if (request.NewPassword != request.ConfirmPassword)
				return Ok(new ApiResponse<string>(false, "Passwords do not match"));

			await _userService.ChangePassword(UserId!, request.NewPassword);
			return Ok(new ApiResponse<string>(true, "Password changed successfully"));
		}
		catch (Exception ex)
		{
			return Ok(new ApiResponse<string>(false, ex.Message));
		}
	}

	[HttpGet("me")]
	[Authorize]
	public async Task<ActionResult<ApiResponse<UserResponse>>> GetCurrentUser()
	{
		try
		{
			if (string.IsNullOrEmpty(UserId))
			{
				return Unauthorized(new ApiResponse<UserResponse>(false, "Invalid token or user not authenticated"));
			}

			var user = await _userService.GetUserById(UserId);
			return Ok(new ApiResponse<UserResponse>(true, "User found", new UserResponse(user.Id.ToString(), user.Username, user.Roles)));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting current user");
			return StatusCode(500, new ApiResponse<UserResponse>(false, "Internal server error"));
		}
	}
}