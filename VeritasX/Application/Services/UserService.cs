using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using VeritasX.Core.Interfaces;
using VeritasX.Infrastructure.Persistence.Entities;

namespace VeritasX.Application.Services;

public class UserService : IUserService
{
	private readonly IUserRepository _userRepository;
	private readonly PasswordHasher<User> _passwordHasher;

	public UserService(IUserRepository userRepository, PasswordHasher<User> passwordHasher)
	{
		_userRepository = userRepository;
		_passwordHasher = passwordHasher;
	}

	public async Task<User> VerifyUserLogin(string username, string password)
	{
		var user = await _userRepository.GetUserByUsername(username);
		if (user == null)
		{
			return user!;
		}

		var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
		if (result == PasswordVerificationResult.Success)
		{
			return user;
		}

		return null!;
	}

	public async Task<User> RegisterUser(string username, string password)
	{
		var existingUser = await _userRepository.GetUserByUsername(username);
		if (existingUser != null)
		{
			throw new ArgumentException("User with this username already exists");
		}

		var newUser = new User
		{
			Username = username,
			Roles = new List<string> { "user" }
		};
		
		newUser.PasswordHash = _passwordHasher.HashPassword(newUser, password);
		
		return await _userRepository.CreateUser(newUser);
	}

	public async Task<User> GetUserById(string userId)
	{
		return await _userRepository.GetUserById(ObjectId.Parse(userId));
	}

	public async Task UpdateUser(User user)
	{
		await _userRepository.UpdateUser(user);
	}

	public async Task ChangePassword(string userId, string newPassword)
	{
		var user = await _userRepository.GetUserById(ObjectId.Parse(userId));
		if (user == null)
		{
			throw new ArgumentException("User not found");
		}

		var hashedPassword = _passwordHasher.HashPassword(user, newPassword);
		await _userRepository.ChangePassword(ObjectId.Parse(userId), hashedPassword);
	}
}