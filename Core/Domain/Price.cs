using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain
{
	public sealed class Price
	{
		public required ExchangeName Exchange { get; init; }
		public required string Symbol { get; init; }
		public decimal LastPrice { get; set; }
		public decimal BidPrice { get; set; }
		public decimal AskPrice { get; set; }
		public DateTimeOffset Timestamp { get; set; }
	}
}
