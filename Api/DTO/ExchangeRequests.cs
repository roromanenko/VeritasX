using Core.Domain;

namespace Api.DTO;

public record PlaceOrderRequest
{
	public required string Symbol { get; init; }
	public required OrderSide Side { get; init; }
	public required OrderType Type { get; init; }
	public required decimal Quantity { get; init; }
	public decimal? Price { get; init; }
}

public record GetCandlesRequest
{
	public required string Symbol { get; init; }
	public string Interval { get; init; } = "1h";
	public DateTime? StartTime { get; init; }
	public DateTime? EndTime { get; init; }
	public int Limit { get; init; } = 100;
}

public record GetTradesRequest
{
	public required string Symbol { get; init; }
	public long? OrderId { get; init; }
	public DateTime? StartTime { get; init; }
	public DateTime? EndTime { get; init; }
	public int Limit { get; init; } = 100;
}
