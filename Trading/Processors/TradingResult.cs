namespace Trading.Processors;

public class TradingResult
{
	public decimal StartTotalInBaseline { get; internal set; }
	public decimal EndTotalInBaseline { get; internal set; }
	public decimal ProfitInBaseline { get; internal set; }
	public decimal JustHoldTotalInBaseline { get; internal set; }
}
