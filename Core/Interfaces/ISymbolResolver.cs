namespace Core.Interfaces;

public interface ISymbolResolver
{
	Task<BinanceSymbolInfo> ParseSymbolAsync(string symbol);
}

public class BinanceSymbolInfo
{
	public required string BaseAsset { get; set; }
	public required string QuoteAsset { get; set; }
	public required string Symbol { get; set; }

	public long ObjectSizeInBytes => sizeof(char) * (BaseAsset.Length + QuoteAsset.Length + Symbol.Length);
}

public class BinanceExchangeInfo
{
	public List<BinanceSymbolInfo> Symbols { get; set; } = new();
}
