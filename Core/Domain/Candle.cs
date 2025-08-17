namespace Core.Domain;

public sealed record  Candle
(
	DateTimeOffset OpenTimeUtc,
	decimal Open,
	decimal High,
	decimal Low,
	decimal Close,
	decimal Volume
);
