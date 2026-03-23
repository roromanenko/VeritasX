namespace Core.Domain;

/// <summary>
/// Identifies the origin of a trade execution.
/// </summary>
public enum TradeSource
{
	/// <summary>
	/// Trade was placed manually by the user through the UI or exchange.
	/// </summary>
	Manual,

	/// <summary>
	/// Trade was executed automatically by a trading bot.
	/// </summary>
	Bot,

	/// <summary>
	/// Trade was placed programmatically via the API.
	/// </summary>
	Api
}
