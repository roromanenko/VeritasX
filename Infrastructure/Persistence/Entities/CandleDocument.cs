namespace Infrastructure.Persistence.Entities;

public record CandleDocument
(
	DateTimeOffset OpenTime,
	decimal Open,
	decimal High,
	decimal Low,
	decimal Close,
	decimal Volume
);
