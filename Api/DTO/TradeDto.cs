namespace Api.DTO
{
	public record TradeDto
	{
		public required string ExchangeTradeId { get; init; }
		public required string ExchangeOrderId { get; init; }
		public required string Symbol { get; init; }
		public required string Side { get; init; }
		public required decimal Price { get; init; }
		public required decimal Quantity { get; init; }
		public required decimal QuoteQuantity { get; init; }
		public required decimal Fee { get; init; }
		public required string FeeAsset { get; init; }
		public required DateTimeOffset ExecutedAt { get; init; }
	}
}
