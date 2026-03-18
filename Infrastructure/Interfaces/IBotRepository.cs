using Infrastructure.Persistence.Entities;
using MongoDB.Bson;

namespace Infrastructure.Interfaces;

public interface IBotRepository
{
	Task<BotConfigurationDocument> CreateBot(BotConfigurationDocument bot);
	Task<BotConfigurationDocument?> GetBotById(ObjectId botId, ObjectId userId);
	Task<BotConfigurationDocument?> GetBotById(ObjectId botId);
	Task<IEnumerable<BotConfigurationDocument>> GetBotsByUserId(ObjectId userId);
	Task<IEnumerable<BotConfigurationDocument>> GetActiveBots();
	Task<IEnumerable<BotConfigurationDocument>> GetPendingBots();
	Task UpdateBot(BotConfigurationDocument bot);
	Task DeleteBot(ObjectId botId, ObjectId userId);
}
