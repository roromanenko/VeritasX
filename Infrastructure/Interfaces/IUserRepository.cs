using Core.Domain;
using Infrastructure.Persistence.Entities;
using MongoDB.Bson;

namespace Infrastructure.Interfaces;

public interface IUserRepository
{
	#region User actions

	Task<UserDocument> CreateUser(UserDocument newUser);
	Task<UserDocument> GetUserByUsername(string username);
	Task<UserDocument> GetUserById(ObjectId userid);
	Task UpdateUser(UserDocument user);
	Task ChangePassword(ObjectId userId, string newPasswordHash);
	Task DeleteUser(ObjectId userId);

	#endregion

	#region Exchange connection actions

	Task AddExchangeConnection(ObjectId userId, ExchangeName exchange, ExchangeConnectionDocument connection);
	Task UpdateExchangeConnection(ObjectId userId, ExchangeName exchange, ExchangeConnectionDocument connection);
	Task RemoveExchangeConnection(ObjectId userId, ExchangeName exchange);
	Task<ExchangeConnectionDocument> GetExchangeConnection(ObjectId userId, ExchangeName exchange);
	Task<Dictionary<ExchangeName, ExchangeConnectionDocument>> GetAllExchangeConnections(ObjectId userId);

	#endregion
}
