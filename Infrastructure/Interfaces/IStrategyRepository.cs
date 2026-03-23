using Infrastructure.Persistence.Entities;
using MongoDB.Bson;

namespace Infrastructure.Interfaces;

public interface IStrategyRepository
{
	Task<StrategyDocument?> GetById(ObjectId id);
	Task<IEnumerable<StrategyDocument>> GetPublic();
	Task<IEnumerable<StrategyDocument>> GetByAuthor(ObjectId authorId);
	Task<StrategyDocument> Create(StrategyDocument strategy);
	Task Update(StrategyDocument strategy);
	Task Delete(ObjectId id);
}
