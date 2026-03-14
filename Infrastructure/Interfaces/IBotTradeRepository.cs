using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Persistence.Entities;
using MongoDB.Bson;

namespace Infrastructure.Interfaces;

public interface IBotTradeRepository
{
	Task<BotTradeRecordDocument> CreateTradeRecord(BotTradeRecordDocument record);
	Task<IEnumerable<BotTradeRecordDocument>> GetTradesByBotId(ObjectId botId, int limit = 100);
	Task<IEnumerable<BotTradeRecordDocument>> GetTradesByUserId(ObjectId userId, int limit = 100);
	Task<IEnumerable<BotTradeRecordDocument>> GetTradesByDateRange(ObjectId botId, DateTimeOffset from, DateTimeOffset to);
}
