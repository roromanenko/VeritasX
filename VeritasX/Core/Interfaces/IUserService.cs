using VeritasX.Infrastructure.Persistence.Entities;

namespace VeritasX.Core.Interfaces;

public interface IUserService
{
	Task<User> VerifyUserLogin(string username, string password);
	Task<User> RegisterUser(string username, string password);
	Task<User> GetUserById(string userId);
	Task UpdateUser(User user);
	Task ChangePassword(string userId, string newPassword);
}