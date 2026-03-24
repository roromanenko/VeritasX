using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Infrastructure.Persistence.Entities;

[Table("bot_open_positions")]
public class BotOpenPositionsDocument
{
	[BsonId]
	public ObjectId Id { get; set; }
	public ObjectId BotId { get; set; }
	public string Symbol { get; set; } = string.Empty;
	public List<OpenPositionEntry> BuyQueue { get; set; } = new();
}

public class OpenPositionEntry
{
	public string TradeId { get; set; } = string.Empty;
	public decimal Price { get; set; }
	public decimal Quantity { get; set; }
	public DateTimeOffset ExecutedAt { get; set; }
}
