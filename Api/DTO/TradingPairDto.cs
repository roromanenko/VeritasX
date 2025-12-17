namespace Api.DTO
{
	public record TradingPairDto
	{
		public required string Symbol { get; init; }
		public required string BaseAsset { get; init; }
		public required string QuoteAsset { get; init; }
		public required decimal MinQuantity { get; init; }
		public required decimal MaxQuantity { get; init; }
		public required decimal QuantityStepSize { get; init; }
		public required decimal MinNotional { get; init; }
		public required decimal MinPrice { get; init; }
		public required decimal MaxPrice { get; init; }
		public required decimal PriceTickSize { get; init; }
		public required bool IsActive { get; init; }
	}
}
