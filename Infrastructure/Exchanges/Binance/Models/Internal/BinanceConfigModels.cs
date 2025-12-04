namespace Infrastructure.Exchanges.Binance.Models.Internal
{
	public class BinanceApiConfig
	{
		public required string BaseUrl { get; set; }
		public required string TestnetBaseUrl { get; set; }
		public int RequestTimeoutSeconds { get; set; } = 30;
		public long RecvWindowMs { get; set; } = 5000;
		public int MaxRetries { get; set; } = 3;
		public int RetryDelayMs { get; set; } = 1000;
	}

	public class BinanceConfig
	{
		public required BinanceApiConfig Api { get; set; }
	}
}
