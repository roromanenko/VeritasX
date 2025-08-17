using AutoMapper;
using Core.Domain;
using Core.Interfaces;
using Infrastructure.Interfaces;
using Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;

namespace Infrastructure.Services;

public class UserService : IUserService
{
	private readonly IUserRepository _userRepository;
	private readonly PasswordHasher<UserEntity> _passwordHasher;
	private readonly IMapper _mapper;

	public UserService(IUserRepository userRepository, PasswordHasher<UserEntity> passwordHasher, IMapper mapper)
	{
		_userRepository = userRepository;
		_passwordHasher = passwordHasher;
		_mapper = mapper;
	}

	public async Task<User?> VerifyUserLogin(string username, string password)
	{
		var userEntity = await _userRepository.GetUserByUsername(username);
		if (userEntity is null)
		{
			return default;
		}

		var result = _passwordHasher.VerifyHashedPassword(userEntity, userEntity.PasswordHash, password);
		if (result == PasswordVerificationResult.Success)
		{
			return _mapper.Map<User>(userEntity);
		}

		return default;
	}

	public async Task<User> RegisterUser(string username, string password)
	{
		var userEntity = await _userRepository.GetUserByUsername(username);
		if (userEntity != null)
		{
			throw new ArgumentException("User with this username already exists");
		}

		var newUser = new UserEntity
		{
			Username = username,
			Roles = ["user"]
		};
		newUser.PasswordHash = _passwordHasher.HashPassword(newUser, password);

		newUser = await _userRepository.CreateUser(newUser);
		return _mapper.Map<User>(newUser);
	}

	public async Task<User> GetUserById(string userId)
	{
		UserEntity userEntity = await _userRepository.GetUserById(ObjectId.Parse(userId));
		return _mapper.Map<User>(userEntity);
	}

	public async Task UpdateUser(User user)
	{
		await _userRepository.UpdateUser(_mapper.Map<UserEntity>(user));
	}

	public async Task ChangePassword(string userId, string newPassword)
	{
		var user = await _userRepository.GetUserById(ObjectId.Parse(userId)) ?? throw new ArgumentException("User not found");
		var hashedPassword = _passwordHasher.HashPassword(user, newPassword);
		await _userRepository.ChangePassword(ObjectId.Parse(userId), hashedPassword);
	}
}