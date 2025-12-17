namespace Api.DTO
{
	public record PortfolioDto
	{
		public required string Exchange { get; init; }
		public required bool IsTestnet { get; init; }
		public required DateTimeOffset UpdatedAt { get; init; }
		public required List<BalanceDto> Balances { get; init; }
	}
}
