using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain
{
	public sealed class Order
	{
		public Order()
		{
			CreatedAt = DateTimeOffset.UtcNow;
			UpdatedAt = DateTimeOffset.UtcNow;
			Status = OrderStatus.Pending;
			FilledQuantity = 0;
		}

		//Identify
		public required string Id { get; set; }
		public string? UserId { get; set; }
		public required string ExchangeOrderId { get; set; }
		public required ExchangeName Exchange { get; set; }

		//Main info
		public required string Symbol { get; set; }
		public OrderSide Side { get; set; }
		public OrderType Type { get; set; }
		public decimal Quantity { get; set; }
		public decimal? QuoteQuantity { get; set; }
		public decimal? Price { get; set; }
		public bool IsTestnet { get; set; }

		//Status
		public OrderStatus Status { get; set; }
		public decimal FilledQuantity { get; set; }
		public decimal? AverageFillPrice { get; set; }

		//Time markers
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset UpdatedAt { get; set; }
		public DateTimeOffset? ExecutedAt { get; set; }
	}

	public enum OrderSide
	{
		Buy,
		Sell
	}

	public enum OrderType
	{
		Market,
		Limit,
		StopLoss,
		StopLimit
	}

	public enum OrderStatus
	{
		Pending,
		New,
		PartiallyFilled,
		Filled,
		Canceled,
		Rejected
	}
}
