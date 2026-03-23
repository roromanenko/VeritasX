using System.Text.Json;
using System.Text.Json.Serialization;
using Trading.Strategies;

namespace Trading;

public class BuiltinStrategyInterpreter : IDslStrategyInterpreter
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		NumberHandling = JsonNumberHandling.AllowReadingFromString
	};

	public ITradingStrategy Build(string snapshot)
	{
		var doc = JsonSerializer.Deserialize<SnapshotDocument>(snapshot, JsonOptions)
			?? throw new ArgumentException("Invalid strategy snapshot JSON");

		return doc.StrategyType switch
		{
			"builtin:rebalance" => BuildRebalanceStrategy(doc),
			_ => throw new NotSupportedException($"Strategy type '{doc.StrategyType}' is not supported.")
		};
	}

	public StrategyMetadata GetMetadata(string snapshot)
	{
		var doc = JsonSerializer.Deserialize<SnapshotDocument>(snapshot, JsonOptions)
			?? throw new ArgumentException("Invalid strategy snapshot JSON");

		var dataReq = Enum.TryParse<DataRequirement>(doc.DataRequirement, true, out var parsed)
			? parsed
			: DataRequirement.Ticker;

		TimeSpan? interval = null;
		if (!string.IsNullOrWhiteSpace(doc.Interval) && TimeSpan.TryParse(doc.Interval, out var ts))
			interval = ts;

		return new StrategyMetadata
		{
			DataRequirement = dataReq,
			Interval = interval
		};
	}

	private static RebalanceToTargetStrategy BuildRebalanceStrategy(SnapshotDocument doc)
	{
		var configJson = doc.Config?.GetRawText() ?? "{}";
		var config = RebalanceConfig.FromJson(configJson);

		if (doc.Overrides is { Count: > 0 })
		{
			var merged = JsonSerializer.Deserialize<Dictionary<string, object>>(configJson, JsonOptions)
				?? new Dictionary<string, object>();

			foreach (var kvp in doc.Overrides)
				merged[kvp.Key] = kvp.Value;

			var mergedJson = JsonSerializer.Serialize(merged, JsonOptions);
			config = RebalanceConfig.FromJson(mergedJson);
		}

		return new RebalanceToTargetStrategy(config);
	}

	private sealed class SnapshotDocument
	{
		public string StrategyType { get; set; } = string.Empty;
		public string DataRequirement { get; set; } = "Ticker";
		public string? Interval { get; set; }
		public JsonElement? Config { get; set; }
		public Dictionary<string, string>? Overrides { get; set; }
	}
}
