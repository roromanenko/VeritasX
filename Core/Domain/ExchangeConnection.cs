using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain
{
	public class ExchangeConnection
	{
		public required ExchangeName ExchangeName { get; set; }
		public required string AccessToken { get; set; }
		public required string RefreshToken { get; set; }
		public int AccessTokenExpiresInSeconds { get; set; }
		public int RefreshTokenExpiresInSeconds { get; set; }
		public string? Scope { get; set; }
		public bool IsTestnet { get; set; } = true;
		public DateTimeOffset RefreshedDate { get; set; } = DateTimeOffset.UtcNow;
	}

	public enum ExchangeName
	{
		Binance,
		Okx,
		Bybit
	}
}
