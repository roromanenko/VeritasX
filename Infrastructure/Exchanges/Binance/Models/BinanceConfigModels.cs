namespace Infrastructure.Exchanges.Binance.Models;

public class BinanceConfig
{
	public required string ApiKey { get; set; }
	public required string SecretKey { get; set; }
	public bool UseTestnet { get; set; } = false;
}
