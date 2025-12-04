using System.Text.Json.Serialization;

namespace Infrastructure.Exchanges.Binance.Models.Api
{
	public class BinanceNewOrderRequest
	{
		[JsonPropertyName("symbol")]
		public required string Symbol { get; set; }

		[JsonPropertyName("side")]
		public required string Side { get; set; }

		[JsonPropertyName("type")]
		public required string Type { get; set; }

		[JsonPropertyName("timeInForce")]
		public string? TimeInForce { get; set; }

		[JsonPropertyName("quantity")]
		public string? Quantity { get; set; }

		[JsonPropertyName("quoteOrderQty")]
		public string? QuoteOrderQty { get; set; }

		[JsonPropertyName("price")]
		public string? Price { get; set; }

		[JsonPropertyName("newClientOrderId")]
		public string? NewClientOrderId { get; set; }

		[JsonPropertyName("stopPrice")]
		public string? StopPrice { get; set; }

		[JsonPropertyName("icebergQty")]
		public string? IcebergQty { get; set; }

		[JsonPropertyName("newOrderRespType")]
		public string? NewOrderRespType { get; set; }

		[JsonPropertyName("recvWindow")]
		public long? RecvWindow { get; set; }

		[JsonPropertyName("timestamp")]
		public long Timestamp { get; set; }
	}

	public class BinanceNewOrderResponse
	{
		[JsonPropertyName("symbol")]
		public required string Symbol { get; set; }

		[JsonPropertyName("orderId")]
		public long OrderId { get; set; }

		[JsonPropertyName("orderListId")]
		public long OrderListId { get; set; }

		[JsonPropertyName("clientOrderId")]
		public required string ClientOrderId { get; set; }

		[JsonPropertyName("transactTime")]
		public long TransactTime { get; set; }

		[JsonPropertyName("price")]
		public required string Price { get; set; }

		[JsonPropertyName("origQty")]
		public required string OrigQty { get; set; }

		[JsonPropertyName("executedQty")]
		public required string ExecutedQty { get; set; }

		[JsonPropertyName("cummulativeQuoteQty")]
		public required string CummulativeQuoteQty { get; set; }

		[JsonPropertyName("status")]
		public required string Status { get; set; }

		[JsonPropertyName("timeInForce")]
		public required string TimeInForce { get; set; }

		[JsonPropertyName("type")]
		public required string Type { get; set; }

		[JsonPropertyName("side")]
		public required string Side { get; set; }

		[JsonPropertyName("fills")]
		public List<BinanceFillResponse> Fills { get; set; } = [];
	}

	public class BinanceFillResponse
	{
		[JsonPropertyName("price")]
		public required string Price { get; set; }

		[JsonPropertyName("qty")]
		public required string Qty { get; set; }

		[JsonPropertyName("commission")]
		public required string Commission { get; set; }

		[JsonPropertyName("commissionAsset")]
		public required string CommissionAsset { get; set; }

		[JsonPropertyName("tradeId")]
		public long TradeId { get; set; }
	}

	public class BinanceQueryOrderRequest
	{
		[JsonPropertyName("symbol")]
		public required string Symbol { get; set; }

		[JsonPropertyName("orderId")]
		public long? OrderId { get; set; }

		[JsonPropertyName("origClientOrderId")]
		public string? OrigClientOrderId { get; set; }

		[JsonPropertyName("recvWindow")]
		public long? RecvWindow { get; set; }

		[JsonPropertyName("timestamp")]
		public long Timestamp { get; set; }
	}

	public class BinanceOrderResponse
	{
		[JsonPropertyName("symbol")]
		public required string Symbol { get; set; }

		[JsonPropertyName("orderId")]
		public long OrderId { get; set; }

		[JsonPropertyName("orderListId")]
		public long OrderListId { get; set; }

		[JsonPropertyName("clientOrderId")]
		public required string ClientOrderId { get; set; }

		[JsonPropertyName("price")]
		public required string Price { get; set; }

		[JsonPropertyName("origQty")]
		public required string OrigQty { get; set; }

		[JsonPropertyName("executedQty")]
		public required string ExecutedQty { get; set; }

		[JsonPropertyName("cummulativeQuoteQty")]
		public required string CummulativeQuoteQty { get; set; }

		[JsonPropertyName("status")]
		public required string Status { get; set; }

		[JsonPropertyName("timeInForce")]
		public required string TimeInForce { get; set; }

		[JsonPropertyName("type")]
		public required string Type { get; set; }

		[JsonPropertyName("side")]
		public required string Side { get; set; }

		[JsonPropertyName("stopPrice")]
		public string? StopPrice { get; set; }

		[JsonPropertyName("icebergQty")]
		public string? IcebergQty { get; set; }

		[JsonPropertyName("time")]
		public long Time { get; set; }

		[JsonPropertyName("updateTime")]
		public long UpdateTime { get; set; }

		[JsonPropertyName("isWorking")]
		public bool IsWorking { get; set; }

		[JsonPropertyName("origQuoteOrderQty")]
		public required string OrigQuoteOrderQty { get; set; }
	}

	public class BinanceCancelOrderRequest
	{
		[JsonPropertyName("symbol")]
		public required string Symbol { get; set; }

		[JsonPropertyName("orderId")]
		public long? OrderId { get; set; }

		[JsonPropertyName("origClientOrderId")]
		public string? OrigClientOrderId { get; set; }

		[JsonPropertyName("newClientOrderId")]
		public string? NewClientOrderId { get; set; }

		[JsonPropertyName("recvWindow")]
		public long? RecvWindow { get; set; }

		[JsonPropertyName("timestamp")]
		public long Timestamp { get; set; }
	}

	public class BinanceCancelOrderResponse
	{
		[JsonPropertyName("symbol")]
		public required string Symbol { get; set; }

		[JsonPropertyName("origClientOrderId")]
		public required string OrigClientOrderId { get; set; }

		[JsonPropertyName("orderId")]
		public long OrderId { get; set; }

		[JsonPropertyName("orderListId")]
		public long OrderListId { get; set; }

		[JsonPropertyName("clientOrderId")]
		public required string ClientOrderId { get; set; }

		[JsonPropertyName("price")]
		public required string Price { get; set; }

		[JsonPropertyName("origQty")]
		public required string OrigQty { get; set; }

		[JsonPropertyName("executedQty")]
		public required string ExecutedQty { get; set; }

		[JsonPropertyName("cummulativeQuoteQty")]
		public required string CummulativeQuoteQty { get; set; }

		[JsonPropertyName("status")]
		public required string Status { get; set; }

		[JsonPropertyName("timeInForce")]
		public required string TimeInForce { get; set; }

		[JsonPropertyName("type")]
		public required string Type { get; set; }

		[JsonPropertyName("side")]
		public required string Side { get; set; }
	}

	public class BinanceOpenOrdersRequest
	{
		[JsonPropertyName("symbol")]
		public string? Symbol { get; set; }

		[JsonPropertyName("recvWindow")]
		public long? RecvWindow { get; set; }

		[JsonPropertyName("timestamp")]
		public long Timestamp { get; set; }
	}

	public class BinanceAllOrdersRequest
	{
		[JsonPropertyName("symbol")]
		public required string Symbol { get; set; }

		[JsonPropertyName("orderId")]
		public long? OrderId { get; set; }

		[JsonPropertyName("startTime")]
		public long? StartTime { get; set; }

		[JsonPropertyName("endTime")]
		public long? EndTime { get; set; }

		[JsonPropertyName("limit")]
		public int? Limit { get; set; }

		[JsonPropertyName("recvWindow")]
		public long? RecvWindow { get; set; }

		[JsonPropertyName("timestamp")]
		public long Timestamp { get; set; }
	}

	public class BinanceTestNewOrderRequest
	{
		[JsonPropertyName("symbol")]
		public required string Symbol { get; set; }

		[JsonPropertyName("side")]
		public required string Side { get; set; }

		[JsonPropertyName("type")]
		public required string Type { get; set; }

		[JsonPropertyName("timeInForce")]
		public string? TimeInForce { get; set; }

		[JsonPropertyName("quantity")]
		public string? Quantity { get; set; }

		[JsonPropertyName("quoteOrderQty")]
		public string? QuoteOrderQty { get; set; }

		[JsonPropertyName("price")]
		public string? Price { get; set; }

		[JsonPropertyName("newClientOrderId")]
		public string? NewClientOrderId { get; set; }

		[JsonPropertyName("stopPrice")]
		public string? StopPrice { get; set; }

		[JsonPropertyName("icebergQty")]
		public string? IcebergQty { get; set; }

		[JsonPropertyName("recvWindow")]
		public long? RecvWindow { get; set; }

		[JsonPropertyName("timestamp")]
		public long Timestamp { get; set; }
	}

	public class BinanceTestNewOrderResponse
	{
		// Empty response object
	}
}
