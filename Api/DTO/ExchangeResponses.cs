namespace Api.DTO;

public record ConnectivityResponse
{
	public required bool IsConnected { get; init; }
	public required DateTimeOffset Timestamp { get; init; }
}

public record ServerTimeResponse
{
	public required long ServerTime { get; init; }
	public required DateTimeOffset ServerDateTime { get; init; }
}

public record OrdersResponse
{
	public required string Symbol { get; init; }
	public required int Count { get; init; }
	public required List<OrderDto> Orders { get; init; }
}

public record TradesResponse
{
	public required string Symbol { get; init; }
	public required int Count { get; init; }
	public required List<TradeDto> Trades { get; init; }
}
