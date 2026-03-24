using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Infrastructure.Persistence.Entities;

[Table("statistics_processed_trades")]
public class StatisticsProcessedTradeDocument
{
	[BsonId]
	public ObjectId Id { get; set; }
	public string TradeId { get; set; } = string.Empty;
	public DateTimeOffset ProcessedAt { get; set; }
}
