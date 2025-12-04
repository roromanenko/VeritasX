using System.Text.Json.Serialization;

namespace Infrastructure.Exchanges.Binance.Models.Api
{
	public class BinanceMyTradesRequest
	{
		[JsonPropertyName("symbol")]
		public required string Symbol { get; set; }

		[JsonPropertyName("orderId")]
		public long? OrderId { get; set; }

		[JsonPropertyName("startTime")]
		public long? StartTime { get; set; }

		[JsonPropertyName("endTime")]
		public long? EndTime { get; set; }

		[JsonPropertyName("fromId")]
		public long? FromId { get; set; }

		[JsonPropertyName("limit")]
		public int? Limit { get; set; }

		[JsonPropertyName("recvWindow")]
		public long? RecvWindow { get; set; }

		[JsonPropertyName("timestamp")]
		public long Timestamp { get; set; }
	}

	public class BinanceTradeResponse
	{
		[JsonPropertyName("symbol")]
		public required string Symbol { get; set; }

		[JsonPropertyName("id")]
		public long Id { get; set; }

		[JsonPropertyName("orderId")]
		public long OrderId { get; set; }

		[JsonPropertyName("orderListId")]
		public long OrderListId { get; set; }

		[JsonPropertyName("price")]
		public required string Price { get; set; }

		[JsonPropertyName("qty")]
		public required string Qty { get; set; }

		[JsonPropertyName("quoteQty")]
		public required string QuoteQty { get; set; }

		[JsonPropertyName("commission")]
		public required string Commission { get; set; }

		[JsonPropertyName("commissionAsset")]
		public required string CommissionAsset { get; set; }

		[JsonPropertyName("time")]
		public long Time { get; set; }

		[JsonPropertyName("isBuyer")]
		public bool IsBuyer { get; set; }

		[JsonPropertyName("isMaker")]
		public bool IsMaker { get; set; }

		[JsonPropertyName("isBestMatch")]
		public bool IsBestMatch { get; set; }
	}
}
