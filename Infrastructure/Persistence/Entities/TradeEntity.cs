using Core.Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Entities
{
	[Table("trade")]
	public class TradeEntity
	{
		[BsonId]
		public ObjectId Id { get; set; }
		public ObjectId? UserId { get; set; }

		[BsonRequired]
		public string Exchange { get; set; } = string.Empty;

		[BsonRequired]
		public string ExchangeOrderId { get; set; } = string.Empty;

		[BsonRequired]
		public string ExchangeTradeId { get; set; } = string.Empty;

		public bool IsTestnet { get; set; }

		[BsonRequired]
		public string Symbol { get; set; } = string.Empty;
		public OrderSide Side { get; set; }
		public decimal Price { get; set; }
		public decimal Quantity { get; set; }
		public decimal QuoteQuantity { get; set; }
		public decimal Fee { get; set; }
		public string FeeAsset { get; set; } = string.Empty;
		public DateTimeOffset ExecutedAt { get; set; }
	}
}
