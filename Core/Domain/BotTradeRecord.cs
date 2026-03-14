namespace Core.Domain;

public class BotTradeRecord
{
	public required string Id { get; init; }
	public required string BotId { get; init; }
	public required string UserId { get; init; }
	public required string Symbol { get; init; }
	public required OrderSide Side { get; init; }
	public required decimal Price { get; init; }
	public required decimal Quantity { get; init; }
	public required string Reason { get; init; }
	public string? TradeId { get; init; }
	public DateTimeOffset ExecutedAt { get; init; } = DateTimeOffset.UtcNow;
}
