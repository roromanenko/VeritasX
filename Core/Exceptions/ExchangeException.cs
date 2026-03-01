namespace Core.Exceptions;

/// <summary>
/// Exception thrown when a requested trading pair is not found on the exchange.
/// </summary>
public class TradingPairNotFoundException : Exception
{
	public TradingPairNotFoundException(string message) : base(message)
	{
	}

	public TradingPairNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
