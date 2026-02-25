namespace Core.Domain;

public class ExchangeConnection
{
	public required string ApiKey { get; set; }
	public required string SecretKey { get; set; }
	public bool IsTestnet { get; set; } = true;
	public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
	public DateTimeOffset? LastUsedAt { get; set; }
}

public enum ExchangeName
{
	Binance,
	Okx,
	Bybit
}
