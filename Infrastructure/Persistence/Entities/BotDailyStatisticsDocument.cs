using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Infrastructure.Persistence.Entities;

[Table("bot_daily_statistics")]
public class BotDailyStatisticsDocument
{
	[BsonId]
	public ObjectId Id { get; set; }
	public ObjectId BotId { get; set; }
	public ObjectId UserId { get; set; }
	public DateOnly Date { get; set; }

	// Equity — mark-to-market
	public decimal OpeningEquity { get; set; }
	public decimal ClosingEquity { get; set; }
	public decimal LastPrice { get; set; }
	public DateTimeOffset LastUpdatedAt { get; set; }

	// P&L — incremented atomically
	public decimal RealizedPnl { get; set; }
	public decimal Fees { get; set; }
	public int TradeCount { get; set; }

	// Round-trip aggregates — incremented atomically per closed position
	public int WinCount { get; set; }
	public int LossCount { get; set; }
	public decimal GrossProfit { get; set; }
	public decimal GrossLoss { get; set; }
	public decimal LargestWin { get; set; }
	public decimal LargestLoss { get; set; }
	public int MaxConsecutiveWins { get; set; }
	public int MaxConsecutiveLosses { get; set; }
	public int CurrentConsecutiveWins { get; set; }
	public int CurrentConsecutiveLosses { get; set; }
}
