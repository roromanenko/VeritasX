namespace Api.DTO;

public class TradeOnHistoryDataRequest
{
	public decimal TargetWeight { get; set; } = .5m;
	public decimal Threshold { get; set; } = .1m;
	public decimal? MinQty { get; set; }
	public decimal? MinNotional { get; set; }

	public required string JobId { get; set; }
	public decimal InitBaselineQuantity { get; set; }
}

public class TradingResultDto
{
	public decimal StartTotalInBaseline { get; set; }
	public decimal EndTotalInBaseline { get; set; }
	public decimal ProfitInBaseline { get; set; }
	public decimal JustHoldTotalInBaseline { get; set; }
}
