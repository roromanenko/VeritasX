using System.ComponentModel.DataAnnotations.Schema;
using Core.Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Infrastructure.Persistence.Entities;

[Table("bot_configurations")]
public class BotConfigurationDocument
{
	[BsonId]
	public ObjectId Id { get; set; }
	public ObjectId UserId { get; set; }
	public string Name { get; set; } = string.Empty;
	public ExchangeName Exchange { get; set; }
	public string Symbol { get; set; } = string.Empty;
	public string BaseAsset { get; set; } = string.Empty;
	public string QuoteAsset { get; set; } = string.Empty;
	public StrategyDefinitionDocument Strategy { get; set; } = null!;
	public RiskParametersDocument RiskParameters { get; set; } = null!;
	public BotStatus Status { get; set; } = BotStatus.Stopped;
	[BsonIgnoreIfNull]
	public string? ErrorMessage { get; set; }
	public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
	[BsonIgnoreIfNull]
	public DateTimeOffset? StartedAt { get; set; }
	[BsonIgnoreIfNull]
	public DateTimeOffset? StoppedAt { get; set; }
}

public class StrategyDefinitionDocument
{
	public StrategyType Type { get; set; }
	public Dictionary<string, string> Parameters { get; set; } = [];
}

public class RiskParametersDocument
{
	public decimal PositionSize { get; set; }
	[BsonIgnoreIfNull]
	public decimal? StopLoss { get; set; }
	[BsonIgnoreIfNull]
	public decimal? TakeProfit { get; set; }
}
