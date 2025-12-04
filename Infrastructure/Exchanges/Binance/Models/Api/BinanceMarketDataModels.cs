using System.Text.Json.Serialization;

namespace Infrastructure.Exchanges.Binance.Models.Api
{
	public class BinanceExchangeInfoRequest
	{
		[JsonPropertyName("symbol")]
		public string? Symbol { get; set; }

		[JsonPropertyName("symbols")]
		public List<string>? Symbols { get; set; }
	}

	public class BinanceExchangeInfoResponse
	{
		[JsonPropertyName("timezone")]
		public required string Timezone { get; set; }

		[JsonPropertyName("serverTime")]
		public long ServerTime { get; set; }

		[JsonPropertyName("symbols")]
		public List<BinanceSymbolInfoResponse> Symbols { get; set; } = [];
	}

	public class BinanceSymbolInfoResponse
	{
		[JsonPropertyName("symbol")]
		public required string Symbol { get; set; }

		[JsonPropertyName("status")]
		public required string Status { get; set; }

		[JsonPropertyName("baseAsset")]
		public required string BaseAsset { get; set; }

		[JsonPropertyName("baseAssetPrecision")]
		public int BaseAssetPrecision { get; set; }

		[JsonPropertyName("quoteAsset")]
		public required string QuoteAsset { get; set; }

		[JsonPropertyName("quotePrecision")]
		public int QuotePrecision { get; set; }

		[JsonPropertyName("orderTypes")]
		public List<string> OrderTypes { get; set; } = [];

		[JsonPropertyName("quoteOrderQtyMarketAllowed")]
		public bool QuoteOrderQtyMarketAllowed { get; set; }

		[JsonPropertyName("isSpotTradingAllowed")]
		public bool IsSpotTradingAllowed { get; set; }

		[JsonPropertyName("filters")]
		public List<BinanceSymbolFilter> Filters { get; set; } = [];
	}

	public class BinanceSymbolFilter
	{
		[JsonPropertyName("filterType")]
		public required string FilterType { get; set; }

		// PRICE_FILTER
		[JsonPropertyName("minPrice")]
		public string? MinPrice { get; set; }

		[JsonPropertyName("maxPrice")]
		public string? MaxPrice { get; set; }

		[JsonPropertyName("tickSize")]
		public string? TickSize { get; set; }

		// LOT_SIZE
		[JsonPropertyName("minQty")]
		public string? MinQty { get; set; }

		[JsonPropertyName("maxQty")]
		public string? MaxQty { get; set; }

		[JsonPropertyName("stepSize")]
		public string? StepSize { get; set; }

		//NOTIONAL
		[JsonPropertyName("minNotional")]
		public string? MinNotional { get; set; }

		[JsonPropertyName("applyMinToMarket")]
		public bool? ApplyMinToMarket { get; set; }

		[JsonPropertyName("maxNotional")]
		public string? MaxNotional { get; set; }

		[JsonPropertyName("applyMaxToMarket")]
		public bool? ApplyMaxToMarket { get; set; }
	}

	public class BinanceTickerPriceRequest
	{
		[JsonPropertyName("symbol")]
		public required string Symbol { get; set; }
	}

	public class BinanceTickerPriceResponse
	{
		[JsonPropertyName("symbol")]
		public required string Symbol { get; set; }

		[JsonPropertyName("price")]
		public required string Price { get; set; }
	}

	public class BinanceTicker24HrRequest
	{
		[JsonPropertyName("symbol")]
		public required string Symbol { get; set; }
	}

	public class BinanceTicker24HrResponse
	{
		[JsonPropertyName("symbol")]
		public required string Symbol { get; set; }

		[JsonPropertyName("lastPrice")]
		public required string LastPrice { get; set; }

		[JsonPropertyName("bidPrice")]
		public required string BidPrice { get; set; }

		[JsonPropertyName("askPrice")]
		public required string AskPrice { get; set; }

		[JsonPropertyName("volume")]
		public required string Volume { get; set; }

		[JsonPropertyName("openTime")]
		public long OpenTime { get; set; }

		[JsonPropertyName("closeTime")]
		public long CloseTime { get; set; }
	}

	public class BinanceKlinesRequest
	{
		[JsonPropertyName("symbol")]
		public required string Symbol { get; set; }

		[JsonPropertyName("interval")]
		public required string Interval { get; set; }

		[JsonPropertyName("startTime")]
		public long? StartTime { get; set; }

		[JsonPropertyName("endTime")]
		public long? EndTime { get; set; }

		[JsonPropertyName("limit")]
		public int? Limit { get; set; }
	}

	public class BinanceKlineResponse
	{
		public long OpenTime { get; set; }
		public required string Open { get; set; }
		public required string High { get; set; }
		public required string Low { get; set; }
		public required string Close { get; set; }
		public required string Volume { get; set; }
		public long CloseTime { get; set; }
	}
}
