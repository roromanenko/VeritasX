using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Domain;

namespace Core.Interfaces;

public interface IBotService
{
	Task<BotConfiguration> CreateBot(string userId, BotConfiguration config);
	Task<BotConfiguration> GetBot(string botId, string userId);
	Task<IEnumerable<BotConfiguration>> GetBots(string userId);
	Task UpdateBot(BotConfiguration bot);
	Task DeleteBot(string botId, string userId);
	Task StartBot(string botId, string userId);
	Task StopBot(string botId, string userId);
	Task<IEnumerable<BotTradeRecord>> GetTradeHistory(string botId, string userId, int limit = 100);
}
