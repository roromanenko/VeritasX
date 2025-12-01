using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain
{
	public sealed class Portfolio
	{
		public required string UserId { get; init; }
		public required ExchangeName Exchange { get; init; }
		public bool IsTestnet { get; init; }
		public DateTimeOffset UpdatedAt { get; set; }
		public List<Balance> Balances { get; set; } = [];
		public decimal TotalUsdValue { get; set; }
	}

	public sealed class Balance
	{
		public string Asset { get; set; } = string.Empty;
		public decimal Free { get; set; }
		public decimal Locked { get; set; }
		public decimal Total => Free + Locked;
	}
}
