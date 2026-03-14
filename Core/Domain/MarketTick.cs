using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain;

public sealed class MarketTick
{
	public required string Symbol { get; init; }
	public required decimal Price { get; init; }
	public Candle? Candle { get; init; }
	public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
