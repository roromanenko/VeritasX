using Core.Domain;
using Infrastructure.Persistence.Entities;
using MongoDB.Bson;

namespace Infrastructure.Interfaces;

public interface IUserRepository
{
	#region User actions

	Task<UserEntity> CreateUser(UserEntity newUser);
	Task<UserEntity> GetUserByUsername(string username);
	Task<UserEntity> GetUserById(ObjectId userid);
	Task UpdateUser(UserEntity user);
	Task ChangePassword(ObjectId userId, string newPasswordHash);
	Task DeleteUser(ObjectId userId);

	#endregion

	#region Exchange connection actions

	Task AddExchangeConnection(ObjectId userId, ExchangeName exchange, ExchangeConnectionEntity connection);
	Task UpdateExchangeConnection(ObjectId userId, ExchangeName exchange, ExchangeConnectionEntity connection);
	Task RemoveExchangeConnection(ObjectId userId, ExchangeName exchange);
	Task<ExchangeConnectionEntity> GetExchangeConnection(ObjectId userId, ExchangeName exchange);
	Task<Dictionary<ExchangeName, ExchangeConnectionEntity>> GetAllExchangeConnections(ObjectId userId);

	#endregion
}
