using System.ComponentModel.DataAnnotations;
using Core.Domain;

namespace Api.DTO;

public record CreateBotRequest(
	string Name,
	ExchangeName Exchange,
	string Symbol,
	string BaseAsset,
	string QuoteAsset,
	string StrategyId,
	Dictionary<string, string>? ParameterOverrides,
	RiskParametersDto RiskParameters,
	[property: Range(1, 100)] int MaxConsecutiveErrors = 5
);

public record UpdateBotRequest(
	string Name,
	string? StrategyId,
	Dictionary<string, string>? ParameterOverrides,
	RiskParametersDto RiskParameters,
	[property: Range(1, 100)] int MaxConsecutiveErrors = 5
);

public record RiskParametersDto(
	decimal PositionSize,
	decimal? StopLoss,
	decimal? TakeProfit
);
