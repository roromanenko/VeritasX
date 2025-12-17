namespace Api.DTO
{
	public record OrderDto
	{
		public required string Id { get; init; }
		public required string ExchangeOrderId { get; init; }
		public required string Symbol { get; init; }
		public required string Side { get; init; }
		public required string Type { get; init; }
		public required string Status { get; init; }
		public required decimal Quantity { get; init; }
		public decimal? QuoteQuantity { get; init; }
		public decimal? Price { get; init; }
		public required decimal FilledQuantity { get; init; }
		public decimal? AverageFillPrice { get; init; }
		public required DateTimeOffset CreatedAt { get; init; }
		public required DateTimeOffset UpdatedAt { get; init; }
		public DateTimeOffset? ExecutedAt { get; init; }
	}
}
