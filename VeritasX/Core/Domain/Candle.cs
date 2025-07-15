namespace VeritasX.Core.Domain;

public record  Candle
(
	DateTime OpenTime,
	decimal Open,
	decimal High,
	decimal Low,
	decimal Close,
	decimal Volume
);
