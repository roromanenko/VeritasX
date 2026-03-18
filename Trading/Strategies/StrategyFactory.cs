using System.Text.Json;
using Core.Domain;

namespace Trading.Strategies;

public class StrategyFactory : IStrategyFactory
{
	public ITradingStrategy Create(StrategyDefinition definition)
	{
		return definition.Type switch
		{
			StrategyType.DeltaRebalancing => new RebalanceToTargetStrategy(
				RebalanceConfig.FromJson(
					JsonSerializer.Serialize(definition.Parameters)
				)
			),
			_ => throw new NotSupportedException($"Strategy type '{definition.Type}' is not supported.")
		};
	}
}
