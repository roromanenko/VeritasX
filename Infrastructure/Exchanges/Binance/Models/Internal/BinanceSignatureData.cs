using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Exchanges.Binance.Models.Internal
{
	public class BinanceSignatureData
	{
		public required string QueryString { get; set; }
		public required string Signature { get; set; }
		public long Timestamp { get; set; }
	}
}
