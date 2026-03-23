using System.ComponentModel.DataAnnotations;
using Core.Domain;

namespace Api.DTO;

public record CreateBotRequest(
	string Name,
	ExchangeName Exchange,
	string Symbol,
	string BaseAsset,
	string QuoteAsset,
	StrategyDefinitionDto Strategy,
	RiskParametersDto RiskParameters,
	[property: Range(1, 100)] int MaxConsecutiveErrors = 5
);

public record UpdateBotRequest(
	string Name,
	Dictionary<string, string> StrategyParameters,
	RiskParametersDto RiskParameters,
	[property: Range(1, 100)] int MaxConsecutiveErrors = 5
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
