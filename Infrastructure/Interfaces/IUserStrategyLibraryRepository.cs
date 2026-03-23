using Infrastructure.Persistence.Entities;
using MongoDB.Bson;

namespace Infrastructure.Interfaces;

public interface IUserStrategyLibraryRepository
{
	Task<IEnumerable<UserStrategyLibraryDocument>> GetByUserId(ObjectId userId);
	Task<UserStrategyLibraryDocument?> GetByUserAndStrategy(ObjectId userId, ObjectId strategyId);
	Task<UserStrategyLibraryDocument> Create(UserStrategyLibraryDocument entry);
	Task Update(UserStrategyLibraryDocument entry);
	Task Delete(ObjectId id);
}
