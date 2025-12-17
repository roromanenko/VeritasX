namespace Api.DTO
{
	public record BalanceDto
	{
		public required string Asset { get; init; }
		public required decimal Free { get; init; }
		public required decimal Locked { get; init; }
		public decimal Total => Free + Locked;
	}
}
