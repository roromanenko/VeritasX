namespace Core.Domain;

public sealed class CandleChunk
{
	public string Id { get; init; } = string.Empty;
	public required string JobId { get; init; }
	public required string Symbol { get; init; }
	public required DateTimeOffset FromUtc { get; init; }
	public required DateTimeOffset ToUtc { get; init; }
	public required TimeSpan Interval { get; init; }

	public List<Candle> Candles { get; init; } = [];
	public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

	public int CandleCount => Candles.Count;
}