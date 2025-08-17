using Api.DTO;
using AutoMapper;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VeritasX.Api.Controllers;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : BaseController
{
	private readonly IUserService _userService;
	private readonly ILogger<UserController> _logger;
	private readonly IJwtService _jwtService;
	private readonly IMapper _mapper;

	public UserController(IUserService userService, ILogger<UserController> logger, IJwtService jwtService, IMapper mapper)
	{
		_userService = userService;
		_logger = logger;
		_jwtService = jwtService;
		_mapper = mapper;
	}

	[HttpPost("register")]
	public async Task<ActionResult<ApiResponse<UserDto>>> Register(RegisterRequest request)
	{
		try
		{
			// Password validation
			if (request.Password != request.ConfirmPassword)
				return Ok(new ApiResponse<UserDto>(false, "Passwords do not match"));

			var user = await _userService.RegisterUser(request.Username, request.Password);

			var response = _mapper.Map<UserDto>(user);
			return Ok(new ApiResponse<UserDto>(true, "User registered successfully", response));
		}
		catch (ArgumentException ex)
		{
			return Ok(new ApiResponse<UserDto>(false, ex.Message));
		}
		catch (Exception)
		{
			return Ok(new ApiResponse<UserDto>(false, "Registration failed"));
		}
	}

	[HttpPost("login")]
	public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(LoginRequest request)
	{
		var user = await _userService.VerifyUserLogin(request.Username, request.Password);
		if (user == null)
			return Ok(new ApiResponse<LoginResponse>(false, "Invalid credentials"));

		var token = _jwtService.GenerateToken(user);

		var userResponse = _mapper.Map<UserDto>(user);
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
	public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
	{
		try
		{
			if (string.IsNullOrEmpty(UserId))
			{
				return Unauthorized(new ApiResponse<UserDto>(false, "Invalid token or user not authenticated"));
			}

			var user = await _userService.GetUserById(UserId);
			var userDto = _mapper.Map<UserDto>(user);
			return Ok(new ApiResponse<UserDto>(true, "User found", userDto));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting current user");
			return StatusCode(500, new ApiResponse<UserDto>(false, "Internal server error"));
		}
	}
}