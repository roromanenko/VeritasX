using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain
{
	/// <summary>
	/// Represents a single executed trade (fill) for an order.<br/> 
	/// Stores execution details such as price, quantity, fees, and timestamps, 
	/// as received from the exchange. Used to track partial or full order executions.
	/// </summary>
	public sealed class Trade
	{
		public required string Id { get; init; }
		public string? UserId { get; set; }
		public required ExchangeName Exchange { get; init; }
		public required string ExchangeOrderId { get; set; }
		public required string ExchangeTradeId { get; set; }
		public bool IsTestnet { get; set; }

		public required string Symbol { get; set; }
		public OrderSide Side { get; set; }

		public decimal Price { get; set; }
		public decimal Quantity { get; set; }
		public decimal QuoteQuantity { get; set; }

		public decimal Fee { get; set; }
		public string FeeAsset { get; set; } = string.Empty;

		public DateTimeOffset ExecutedAt { get; set; }
	}
}
