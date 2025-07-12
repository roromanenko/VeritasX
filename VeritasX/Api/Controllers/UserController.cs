using Microsoft.AspNetCore.Mvc;
using VeritasX.Application.Services;
using VeritasX.Core.Interfaces;
using VeritasX.Infrastructure.Persistence.Entities;
using VeritasX.Core.DTO;
using Microsoft.AspNetCore.Authorization;

namespace VeritasX.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : BaseController
{
    private readonly IUserService _userService;
	private readonly ILogger<UserController> _logger;

	public UserController(IUserService userService, ILogger<UserController> logger)
	{
		_userService = userService;
		_logger = logger;
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
		
		await LoginUser(user);
		
		var response = new LoginResponse(
			new UserResponse(user.Id.ToString(), user.Username, user.Roles)
		);
		return Ok(new ApiResponse<LoginResponse>(true, "Login successful", response));
	}
    
    [HttpPut("password")]
	[Authorize]
	public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
	{
		try
		{
			var user = await _userService.VerifyUserLogin(Username, request.CurrentPassword);
			if (user == null)
				return Ok(new ApiResponse<object>(false, "Current password is incorrect"));

			if (request.NewPassword != request.ConfirmPassword)
				return Ok(new ApiResponse<object>(false, "Passwords do not match"));

			await _userService.ChangePassword(UserId, request.NewPassword);
			return Ok(new ApiResponse<object>(true, "Password changed successfully"));
		}
		catch (Exception ex)
		{
			return Ok(new ApiResponse<object>(false, ex.Message));
		}
	}

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetCurrentUser()
    {
        var user = await _userService.GetUserById(UserId);
        if (user == null)
            return Ok(new ApiResponse<UserResponse>(false, "User not found"));
        
        var response = new UserResponse(user.Id.ToString(), user.Username, user.Roles);
        return Ok(new ApiResponse<UserResponse>(true, "User found", response));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await LogoutUser();
        return Ok(new ApiResponse<object>(true, "Logged out successfully"));
    }

    [HttpGet("profile")]
    [Authorize]
    public IActionResult GetProfile()
    { 
        var currentUserId = UserId;
        var currentUsername = Username;
        
        return Ok($"Hello {currentUsername}!");
    }
} 