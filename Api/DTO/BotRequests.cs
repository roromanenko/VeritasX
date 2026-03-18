using Core.Domain;

namespace Api.DTO;

public record CreateBotRequest(
	string Name,
	ExchangeName Exchange,
	string Symbol,
	string BaseAsset,
	string QuoteAsset,
	StrategyDefinitionDto Strategy,
	RiskParametersDto RiskParameters
);

public record StrategyDefinitionDto(
	StrategyType Type,
	Dictionary<string, string> Parameters
);

public record RiskParametersDto(
	decimal PositionSize,
	decimal? StopLoss,
	decimal? TakeProfit
);
