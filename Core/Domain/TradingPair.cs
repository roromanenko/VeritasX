namespace Core.Domain
{
	public sealed class TradingPair
	{
		public required ExchangeName Exchange { get; init; }

		/// <summary>
		/// Example: "BTCUSDT" or "ETHUSDT"
		/// </summary>
		public required string Symbol { get; init; }

		/// <summary>
		/// Example: "BTC" or "ETH"
		/// </summary>
		public required string BaseAsset { get; init; }

		/// <summary>
		/// Example: "USDT"
		/// </summary>
		public required string QuoteAsset { get; init; }

		public decimal MinQuantity { get; set; }
		public decimal MaxQuantity { get; set; }
		public decimal QuantityStepSize { get; set; }

		public decimal MinNotional { get; set; }
		public decimal MinPrice { get; set; }
		public decimal MaxPrice { get; set; }
		public decimal PriceTickSize { get; set; }

		public bool IsActive { get; set; }
		public DateTimeOffset UpdatedAt { get; set; }
	}
}
