using Core.Domain;
using Infrastructure.Persistence.Entities;
using MongoDB.Bson;

namespace Infrastructure.Interfaces;

public interface IUserRepository
{
	Task<UserEntity> CreateUser(UserEntity newUser);
	Task<UserEntity> GetUserByUsername(string username);
	Task<UserEntity> GetUserById(ObjectId userid);
	Task UpdateUser(UserEntity user);
	Task ChangePassword(ObjectId userId, string newPasswordHash);
}