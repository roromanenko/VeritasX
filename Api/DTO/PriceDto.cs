namespace Api.DTO
{
	public record PriceDto
	{
		public required string Symbol { get; init; }
		public required decimal LastPrice { get; init; }
		public required decimal BidPrice { get; init; }
		public required decimal AskPrice { get; init; }
		public required DateTimeOffset Timestamp { get; init; }
	}
}
