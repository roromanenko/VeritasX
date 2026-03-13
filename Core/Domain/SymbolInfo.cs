namespace Core.Domain;

public class SymbolInfo
{
	public required string BaseAsset { get; init; }
	public required string QuoteAsset { get; init; }
	public required string Symbol { get; init; }
}
