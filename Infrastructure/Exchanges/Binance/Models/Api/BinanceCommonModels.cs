using System.Text.Json.Serialization;

namespace Infrastructure.Exchanges.Binance.Models.Api
{
	public class BinanceErrorResponse
	{
		[JsonPropertyName("code")]
		public int Code { get; set; }

		[JsonPropertyName("msg")]
		public required string Msg { get; set; }
	}

	public class BinanceSuccessResponse
	{
		[JsonPropertyName("code")]
		public int? Code { get; set; }

		[JsonPropertyName("success")]
		public bool Success { get; set; }

		[JsonPropertyName("msg")]
		public string? Msg { get; set; }
	}

	public class BinanceServerTimeResponse
	{
		[JsonPropertyName("serverTime")]
		public long ServerTime { get; set; }
	}

	public class BinancePingResponse
	{
		// Empty response object
	}

	public class BinanceTestConnectivityResponse
	{
		// Empty response object
	}
}
