using Core.Domain;

namespace Core.Interfaces;

public interface IUserService
{
	#region User actions

	Task<User?> VerifyUserLogin(string username, string password);
	Task<User> RegisterUser(string username, string password);
	Task<User> GetUserById(string userId);
	Task UpdateUser(User user);
	Task ChangePassword(string userId, string newPassword);

	#endregion

	#region Exchange connection actions

	Task AddExchangeConnection(string userId, ExchangeName exchange, ExchangeConnection connection);
	Task UpdateExchangeConnection(string userId, ExchangeName exchange, ExchangeConnection connection);
	Task RemoveExchangeConnection(string userId, ExchangeName exchange);
	Task<ExchangeConnection> GetExchangeConnection(string userId, ExchangeName exchange);
	Task<Dictionary<ExchangeName, ExchangeConnection>> GetAllExchangeConnections(string userId);

	#endregion
}
