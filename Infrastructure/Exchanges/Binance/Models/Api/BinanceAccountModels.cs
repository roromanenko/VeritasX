using System.Text.Json.Serialization;

namespace Infrastructure.Exchanges.Binance.Models.Api
{
	public class BinanceAccountInfoResponse
	{
		[JsonPropertyName("makerCommission")]
		public int MakerCommission { get; set; }

		[JsonPropertyName("takerCommission")]
		public int TakerCommission { get; set; }

		[JsonPropertyName("buyerCommission")]
		public int BuyerCommission { get; set; }

		[JsonPropertyName("sellerCommission")]
		public int SellerCommission { get; set; }

		[JsonPropertyName("canTrade")]
		public bool CanTrade { get; set; }

		[JsonPropertyName("canWithdraw")]
		public bool CanWithdraw { get; set; }

		[JsonPropertyName("canDeposit")]
		public bool CanDeposit { get; set; }

		[JsonPropertyName("updateTime")]
		public long UpdateTime { get; set; }

		[JsonPropertyName("accountType")]
		public required string AccountType { get; set; }

		[JsonPropertyName("balances")]
		public List<BinanceBalanceResponse> Balances { get; set; } = [];

		[JsonPropertyName("permissions")]
		public List<string> Permissions { get; set; } = [];
	}

	public class BinanceBalanceResponse
	{
		[JsonPropertyName("asset")]
		public required string Asset { get; set; }

		[JsonPropertyName("free")]
		public required string Free { get; set; }

		[JsonPropertyName("locked")]
		public required string Locked { get; set; }
	}
}
