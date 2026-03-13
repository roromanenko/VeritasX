using Core.Domain;

namespace Core.Interfaces;

public interface ISymbolResolver
{
	Task<SymbolInfo> ParseSymbolAsync(string symbol);
}
