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
	private readonly PasswordHasher<UserDocument> _passwordHasher;
	private readonly IMapper _mapper;
	private readonly IEncryptionService _encryptionService;

	public UserService(IUserRepository userRepository, PasswordHasher<UserDocument> passwordHasher, IMapper mapper, IEncryptionService encryptionService)
	{
		_userRepository = userRepository;
		_passwordHasher = passwordHasher;
		_mapper = mapper;
		_encryptionService = encryptionService;
	}

	#region User actions

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

		var newUser = new UserDocument
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
		UserDocument userEntity = await _userRepository.GetUserById(ObjectId.Parse(userId));

		return userEntity is null
			? throw new KeyNotFoundException($"User with ID '{userId}' was not found.")
			: _mapper.Map<User>(userEntity);
	}

	public async Task UpdateUser(User user)
	{
		await _userRepository.UpdateUser(_mapper.Map<UserDocument>(user));
	}

	public async Task ChangePassword(string userId, string newPassword)
	{
		var user = await _userRepository.GetUserById(ObjectId.Parse(userId)) ?? throw new ArgumentException("User not found");
		var hashedPassword = _passwordHasher.HashPassword(user, newPassword);
		await _userRepository.ChangePassword(ObjectId.Parse(userId), hashedPassword);
	}

	#endregion

	#region Exchange connection actions

	public async Task AddExchangeConnection(string userId, ExchangeName exchange, ExchangeConnection connection)
	{
		var entity = EncryptConnection(connection);
		await _userRepository.AddExchangeConnection(ObjectId.Parse(userId), exchange, entity);
	}

	public async Task UpdateExchangeConnection(string userId, ExchangeName exchange, ExchangeConnection connection)
	{
		var entity = EncryptConnection(connection);
		await _userRepository.UpdateExchangeConnection(ObjectId.Parse(userId), exchange, entity);
	}

	public async Task RemoveExchangeConnection(string userId, ExchangeName exchange)
	{
		await _userRepository.RemoveExchangeConnection(ObjectId.Parse(userId), exchange);
	}

	public async Task<ExchangeConnection> GetExchangeConnection(string userId, ExchangeName exchange)
	{
		var entity = await _userRepository.GetExchangeConnection(ObjectId.Parse(userId), exchange);
		return DecryptConnection(entity);
	}

	public async Task<Dictionary<ExchangeName, ExchangeConnection>> GetAllExchangeConnections(string userId)
	{
		var entities = await _userRepository.GetAllExchangeConnections(ObjectId.Parse(userId));
		return entities.ToDictionary(kvp => kvp.Key, kvp => DecryptConnection(kvp.Value));
	}

	private ExchangeConnectionDocument EncryptConnection(ExchangeConnection connection) => new()
	{
		EncryptedApiKey = _encryptionService.Encrypt(connection.ApiKey),
		EncryptedSecretKey = _encryptionService.Encrypt(connection.SecretKey),
		IsTestnet = connection.IsTestnet,
		CreatedAt = connection.CreatedAt,
		LastUsedAt = connection.LastUsedAt
	};

	private ExchangeConnection DecryptConnection(ExchangeConnectionDocument entity) => new()
	{
		ApiKey = _encryptionService.Decrypt(entity.EncryptedApiKey),
		SecretKey = _encryptionService.Decrypt(entity.EncryptedSecretKey),
		IsTestnet = entity.IsTestnet,
		CreatedAt = entity.CreatedAt,
		LastUsedAt = entity.LastUsedAt
	};

	#endregion
}
