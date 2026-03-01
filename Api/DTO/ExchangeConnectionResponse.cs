using Core.Domain;

public record ExchangeConnectionResponse(
	ExchangeName Exchange,
	bool IsTestnet,
	DateTimeOffset CreatedAt,
	DateTimeOffset? LastUsedAt
);
