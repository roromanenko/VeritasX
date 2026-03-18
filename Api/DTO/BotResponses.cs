using Core.Domain;

namespace Api.DTO;

public record BotDto(
	string Id,
	string Name,
	ExchangeName Exchange,
	string Symbol,
	string BaseAsset,
	string QuoteAsset,
	BotStatus Status,
	StrategyDefinitionDto Strategy,
	RiskParametersDto RiskParameters,
	DateTimeOffset CreatedAt,
	DateTimeOffset? StartedAt,
	DateTimeOffset? StoppedAt,
	string? ErrorMessage
);

public record BotTradeRecordDto(
	string Id,
	string BotId,
	string Symbol,
	OrderSide Side,
	decimal Price,
	decimal Quantity,
	string Reason,
	DateTimeOffset ExecutedAt
);
