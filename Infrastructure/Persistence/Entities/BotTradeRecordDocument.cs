using Core.Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Infrastructure.Persistence.Entities;

public class BotTradeRecordDocument
{
	[BsonId]
	public ObjectId Id { get; set; }
	public ObjectId BotId { get; set; }
	public ObjectId UserId { get; set; }
	public string Symbol { get; set; } = string.Empty;
	public OrderSide Side { get; set; }
	public decimal Price { get; set; }
	public decimal Quantity { get; set; }
	public string Reason { get; set; } = string.Empty;
	[BsonIgnoreIfNull]
	public ObjectId? TradeId { get; set; }
	public DateTimeOffset ExecutedAt { get; set; }
}
