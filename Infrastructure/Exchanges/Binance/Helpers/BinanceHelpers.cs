using Core.Domain;
using BinanceNet = Binance.Net;
using Binance.Net.Enums;
using Binance.Net.Objects.Models.Spot;
using System.Globalization;

namespace Infrastructure.Exchanges.Binance.Helpers
{
	public static class BinanceHelpers
	{
		//Order side conversions
		public static Core.Domain.OrderSide ToBinanceSide(BinanceNet.Enums.OrderSide side)
		{
			return side switch
			{
				BinanceNet.Enums.OrderSide.Buy => Core.Domain.OrderSide.Buy,
				BinanceNet.Enums.OrderSide.Sell => Core.Domain.OrderSide.Sell,
				_ => throw new ArgumentException($"Unknown Binance order side: {side}")
			};
		}

		public static BinanceNet.Enums.OrderSide FromDomainSide(Core.Domain.OrderSide side)
		{
			return side switch
			{
				Core.Domain.OrderSide.Buy => BinanceNet.Enums.OrderSide.Buy,
				Core.Domain.OrderSide.Sell => BinanceNet.Enums.OrderSide.Sell,
				_ => throw new ArgumentException($"Unknown order side: {side}")
			};
		}

		//Order type conversions
		public static Core.Domain.OrderType ToBinanceOrderType(SpotOrderType type)
		{
			return type switch
			{
				SpotOrderType.Market => Core.Domain.OrderType.Market,
				SpotOrderType.Limit => Core.Domain.OrderType.Limit,
				SpotOrderType.StopLoss => Core.Domain.OrderType.StopLoss,
				SpotOrderType.StopLossLimit => Core.Domain.OrderType.StopLimit,
				_ => throw new ArgumentException($"Unknown Binance order type: {type}")
			};
		}

		public static SpotOrderType FromDomainOrderType(Core.Domain.OrderType type)
		{
			return type switch
			{
				Core.Domain.OrderType.Market => SpotOrderType.Market,
				Core.Domain.OrderType.Limit => SpotOrderType.Limit,
				Core.Domain.OrderType.StopLoss => SpotOrderType.StopLoss,
				Core.Domain.OrderType.StopLimit => SpotOrderType.StopLossLimit,
				_ => throw new ArgumentException($"Unknown order type: {type}")
			};
		}

		// Order status conversions
		public static Core.Domain.OrderStatus ToBinanceStatus(BinanceNet.Enums.OrderStatus status)
		{
			return status switch
			{
				BinanceNet.Enums.OrderStatus.New => Core.Domain.OrderStatus.New,
				BinanceNet.Enums.OrderStatus.PartiallyFilled => Core.Domain.OrderStatus.PartiallyFilled,
				BinanceNet.Enums.OrderStatus.Filled => Core.Domain.OrderStatus.Filled,
				BinanceNet.Enums.OrderStatus.Canceled => Core.Domain.OrderStatus.Canceled,
				BinanceNet.Enums.OrderStatus.PendingCancel => Core.Domain.OrderStatus.Canceled,
				BinanceNet.Enums.OrderStatus.Rejected => Core.Domain.OrderStatus.Rejected,
				BinanceNet.Enums.OrderStatus.Expired => Core.Domain.OrderStatus.Canceled,
				_ => throw new ArgumentException($"Unknown Binance status: {status}")
			};
		}

		//Symbol filter helpers
		public static decimal GetMinQuantity(BinanceSymbol symbol)
		{
			var lotSize = symbol.LotSizeFilter;
			return lotSize?.MinQuantity ?? 0m;
		}

		public static decimal GetMaxQuantity(BinanceSymbol symbol)
		{
			var lotSize = symbol.LotSizeFilter;
			return lotSize?.MaxQuantity ?? decimal.MaxValue;
		}

		/// <summary>
		/// Gets the quantity step size (increment) for the trading pair.
		/// </summary>
		public static decimal GetStepSize(BinanceSymbol symbol)
		{
			var lotSize = symbol.LotSizeFilter;
			return lotSize?.StepSize ?? 0m;
		}

		public static decimal GetMinPrice(BinanceSymbol symbol)
		{
			var priceFilter = symbol.PriceFilter;
			return priceFilter?.MinPrice ?? 0m;
		}

		public static decimal GetMaxPrice(BinanceSymbol symbol)
		{
			var priceFilter = symbol.PriceFilter;
			return priceFilter?.MaxPrice ?? decimal.MaxValue;
		}

		/// <summary>
		/// Gets the price tick size (increment) for the trading pair.
		/// </summary>
		public static decimal GetTickSize(BinanceSymbol symbol)
		{
			var priceFilter = symbol.PriceFilter;
			return priceFilter?.TickSize ?? 0m;
		}

		public static decimal GetMinNotional(BinanceSymbol symbol, bool isMarket = false)
		{
			var notional = symbol.MinNotionalFilter;
			if (notional == null)
				return 0m;

			if (isMarket && notional.ApplyToMarketOrders == false)
				return 0m;

			return notional.MinNotional;
		}

		//Precision helpers
		/// <summary>
		/// Calculates decimal precision (number of digits after decimal point) from step size.
		/// </summary>
		private static int GetPrecision(decimal stepSize)
		{
			var str = stepSize.ToString(CultureInfo.InvariantCulture);
			var decimalIndex = str.IndexOf('.');
			if (decimalIndex == -1)
				return 0;

			var afterDecimal = str.Substring(decimalIndex + 1).TrimEnd('0');
			return afterDecimal.Length;
		}

		//Quantity/Price rounding
		/// <summary>
		/// Rounds quantity down to the nearest valid value according to step size.
		/// </summary>
		public static decimal RoundQuantity(decimal quantity, decimal stepSize)
		{
			if (stepSize == 0) return quantity;

			var precision = GetPrecision(stepSize);
			var rounded = Math.Floor(quantity / stepSize) * stepSize;
			return Math.Round(rounded, precision);
		}

		/// <summary>
		/// Rounds price down to the nearest valid value according to tick size.
		/// </summary>
		public static decimal RoundPrice(decimal price, decimal tickSize)
		{
			if (tickSize == 0) return price;

			var precision = GetPrecision(tickSize);
			var rounded = Math.Floor(price / tickSize) * tickSize;
			return Math.Round(rounded, precision);
		}

		// Validation
		public static bool ValidateQuantity(decimal quantity, BinanceSymbol symbol)
		{
			var minQty = GetMinQuantity(symbol);
			var maxQty = GetMaxQuantity(symbol);
			var stepSize = GetStepSize(symbol);

			if (quantity < minQty || quantity > maxQty)
				return false;

			if (stepSize > 0)
			{
				var rounded = RoundQuantity(quantity, stepSize);
				var precision = GetPrecision(stepSize);

				if (Math.Round(quantity, precision) != rounded)
					return false;
			}

			return true;
		}

		public static bool ValidatePrice(decimal price, BinanceSymbol symbol)
		{
			var minPrice = GetMinPrice(symbol);
			var maxPrice = GetMaxPrice(symbol);
			var tickSize = GetTickSize(symbol);

			if (price < minPrice || price > maxPrice)
				return false;

			if (tickSize > 0)
			{
				var rounded = RoundPrice(price, tickSize);
				var precision = GetPrecision(tickSize);

				if (Math.Round(price, precision) != rounded)
					return false;
			}

			return true;
		}

		public static bool ValidateNotional(decimal price, decimal quantity, BinanceSymbol symbol, bool isMarket = false)
		{
			var minNotional = GetMinNotional(symbol, isMarket);
			var notional = price * quantity;
			return notional >= minNotional;
		}

		public static BinanceNet.Enums.KlineInterval ParseInterval(string interval)
		{
			return interval switch
			{
				"1m" => BinanceNet.Enums.KlineInterval.OneMinute,
				"3m" => BinanceNet.Enums.KlineInterval.ThreeMinutes,
				"5m" => BinanceNet.Enums.KlineInterval.FiveMinutes,
				"15m" => BinanceNet.Enums.KlineInterval.FifteenMinutes,
				"30m" => BinanceNet.Enums.KlineInterval.ThirtyMinutes,
				"1h" => BinanceNet.Enums.KlineInterval.OneHour,
				"2h" => BinanceNet.Enums.KlineInterval.TwoHour,
				"4h" => BinanceNet.Enums.KlineInterval.FourHour,
				"6h" => BinanceNet.Enums.KlineInterval.SixHour,
				"8h" => BinanceNet.Enums.KlineInterval.EightHour,
				"12h" => BinanceNet.Enums.KlineInterval.TwelveHour,
				"1d" => BinanceNet.Enums.KlineInterval.OneDay,
				"3d" => BinanceNet.Enums.KlineInterval.ThreeDay,
				"1w" => BinanceNet.Enums.KlineInterval.OneWeek,
				"1M" => BinanceNet.Enums.KlineInterval.OneMonth,
				_ => throw new ArgumentException($"Unknown interval: {interval}")
			};
		}
	}
}
