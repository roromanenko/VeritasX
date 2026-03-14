using AutoMapper;
using Core.Domain;
using Core.Interfaces;
using Infrastructure.Interfaces;
using Infrastructure.Persistence.Entities;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using Moq;

namespace Infrastructure.Tests.ServicesTests;

public class UserServiceTests
{
	private readonly Mock<IUserRepository> _userRepositoryMock;
	private readonly Mock<IMapper> _mapperMock;
	private readonly Mock<IEncryptionService> _encryptionServiceMock;
	private readonly PasswordHasher<UserDocument> _passwordHasher;
	private readonly UserService _sut;

	public UserServiceTests()
	{
		_userRepositoryMock = new Mock<IUserRepository>();
		_mapperMock = new Mock<IMapper>();
		_encryptionServiceMock = new Mock<IEncryptionService>();
		_passwordHasher = new PasswordHasher<UserDocument>();

		_sut = new UserService(
			_userRepositoryMock.Object,
			_passwordHasher,
			_mapperMock.Object,
			_encryptionServiceMock.Object);
	}

	[Theory]
	[InlineData("existingUser", "super_secret_password")]
	public async Task RegisterUser_WhenUserAlreadyExists_ShouldThrowArgumentException(string username, string password)
	{
		//Arrange
		var existingUser = new UserDocument { Username = username };
		_userRepositoryMock
			.Setup(repo => repo.GetUserByUsername(username))
			.ReturnsAsync(existingUser);

		//Act&Assert
		var exception = await Assert.ThrowsAsync<ArgumentException>(() => _sut.RegisterUser(username, password));

		Assert.Equal("User with this username already exists", exception.Message);
		_userRepositoryMock.Verify(repo => repo.CreateUser(It.IsAny<UserDocument>()), Times.Never);
	}

	[Fact]
	public async Task RegisterUser_WhenUserIsNew_ShouldReturnMappedUser()
	{
		//Arrange
		var username = "NewUser";
		var password = "super_secret_password";

		_userRepositoryMock
			.Setup(repo => repo.GetUserByUsername(username))
			.ReturnsAsync((UserDocument?)null!);

		_userRepositoryMock
			.Setup(repo => repo.CreateUser(It.IsAny<UserDocument>()))
			.ReturnsAsync((UserDocument u) =>
			{
				u.Id = ObjectId.GenerateNewId();
				return u;
			});

		_mapperMock
			.Setup(m => m.Map<User>(It.IsAny<UserDocument>()))
			.Returns((UserDocument entity) => new User
			{
				Id = entity.Id.ToString(),
				Username = entity.Username,
				PasswordHash = entity.PasswordHash,
				Roles = [.. entity.Roles]
			});

		//Act
		var result = await _sut.RegisterUser(username, password);

		//Assert
		Assert.NotNull(result);
		Assert.Equal(username, result.Username);
		Assert.False(string.IsNullOrEmpty(result.Id));
		Assert.False(string.IsNullOrEmpty(result.PasswordHash));
		Assert.Contains("user", result.Roles);
		Assert.NotEqual(password, result.PasswordHash);

		_userRepositoryMock.Verify(repo => repo.GetUserByUsername(username), Times.Once);
		_userRepositoryMock.Verify(repo => repo.CreateUser(It.IsAny<UserDocument>()), Times.Once);
		_mapperMock.Verify(m => m.Map<User>(It.IsAny<UserDocument>()), Times.Once);
	}

	[Theory]
	[InlineData("507f1f77bcf86cd799439011")]
	public async Task GetUserByID_WhenUserMissing_ShouldThrowKeyNotFoundException(string userId)
	{
		//Arrange
		_userRepositoryMock
			.Setup(repo => repo.GetUserById(It.IsAny<ObjectId>()))
			.ReturnsAsync((UserDocument?)null!);

		//Act
		var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetUserById(userId));

		//Assert
		Assert.Equal($"User with ID '{userId}' was not found.", exception.Message);
		_userRepositoryMock.Verify(repo => repo.GetUserById(It.IsAny<ObjectId>()), Times.Once);
		_mapperMock.Verify(mapper => mapper.Map<User>(It.IsAny<UserDocument>()), Times.Never);
	}

	[Theory]
	[InlineData("507f1f77bcf86cd799439011")]
	public async Task GetUserByID_WhenIdCorrect_ShouldReturnMappedUser(string userId)
	{
		//Arrange
		var password = "super_secret_password";
		var userEntity = new UserDocument
		{
			Id = ObjectId.Parse(userId),
			Username = "test_user",
			PasswordHash = _passwordHasher.HashPassword(null!, password),
			Roles = ["user"]
		};

		_userRepositoryMock
			.Setup(repo => repo.GetUserById(ObjectId.Parse(userId)))
			.ReturnsAsync(userEntity);

		_mapperMock
			.Setup(m => m.Map<User>(It.IsAny<UserDocument>()))
			.Returns((UserDocument entity) => new User
			{
				Id = entity.Id.ToString(),
				Username = entity.Username,
				PasswordHash = entity.PasswordHash,
				Roles = [.. entity.Roles]
			});

		//Act
		var result = await _sut.GetUserById(userId);

		//Assert
		Assert.NotNull(result);
		Assert.Equal(userEntity.Username, result.Username);
		Assert.Equal(userEntity.Id.ToString(), result.Id);
		Assert.False(string.IsNullOrEmpty(result.PasswordHash));
		Assert.Contains("user", result.Roles);
		Assert.NotEqual(password, result.PasswordHash);

		_userRepositoryMock.Verify(repo => repo.GetUserById(ObjectId.Parse(userId)), Times.Once);
		_mapperMock.Verify(m => m.Map<User>(userEntity), Times.Once);
	}
}
