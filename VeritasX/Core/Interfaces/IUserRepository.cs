using VeritasX.Infrastructure.Persistence.Entities;
using MongoDB.Bson;

namespace VeritasX.Core.Interfaces;

public interface IUserRepository
{
	Task<User> CreateUser(User newUser);
	Task<User> GetUserByUsername(string username);
	Task<User> GetUserById(ObjectId userid);
	Task UpdateUser(User user);
	Task ChangePassword(ObjectId userId, string newPasswordHash);
}