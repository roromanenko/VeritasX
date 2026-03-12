using System.Globalization;
using Binance.Net.Enums;
using Binance.Net.Objects.Models.Spot;
using Core.Domain;
using BinanceNet = Binance.Net;

namespace Infrastructure.Exchanges.Binance.Helpers;

public static class BinanceHelpers
{
	//Order side conversions
	public static Core.Domain.OrderSide ToDomainSide(BinanceNet.Enums.OrderSide side)
	{
		return side switch
		{
			BinanceNet.Enums.OrderSide.Buy => Core.Domain.OrderSide.Buy,
			BinanceNet.Enums.OrderSide.Sell => Core.Domain.OrderSide.Sell,
			_ => throw new ArgumentException($"Unknown Binance order side: {side}")
		};
	}

	public static BinanceNet.Enums.OrderSide ToBinanceSide(Core.Domain.OrderSide side)
	{
		return side switch
		{
			Core.Domain.OrderSide.Buy => BinanceNet.Enums.OrderSide.Buy,
			Core.Domain.OrderSide.Sell => BinanceNet.Enums.OrderSide.Sell,
			_ => throw new ArgumentException($"Unknown order side: {side}")
		};
	}

	//Order type conversions
	public static Core.Domain.OrderType ToDomainOrderType(SpotOrderType type)
	{
		return type switch
		{
			SpotOrderType.Market => OrderType.Market,
			SpotOrderType.Limit => OrderType.Limit,
			SpotOrderType.StopLoss => OrderType.StopLoss,
			SpotOrderType.StopLossLimit => OrderType.StopLimit,
			_ => throw new ArgumentException($"Unknown Binance order type: {type}")
		};
	}

	public static SpotOrderType ToBinanceOrderType(Core.Domain.OrderType type)
	{
		return type switch
		{
			OrderType.Market => SpotOrderType.Market,
			OrderType.Limit => SpotOrderType.Limit,
			OrderType.StopLoss => SpotOrderType.StopLoss,
			OrderType.StopLimit => SpotOrderType.StopLossLimit,
			_ => throw new ArgumentException($"Unknown order type: {type}")
		};
	}

	// Order status conversions
	public static Core.Domain.OrderStatus ToDomainStatus(BinanceNet.Enums.OrderStatus status)
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

	public static KlineInterval ToKlineInterval(TimeSpan interval)
	{
		return interval.TotalMinutes switch
		{
			1 => KlineInterval.OneMinute,
			3 => KlineInterval.ThreeMinutes,
			5 => KlineInterval.FiveMinutes,
			15 => KlineInterval.FifteenMinutes,
			30 => KlineInterval.ThirtyMinutes,
			60 => KlineInterval.OneHour,
			120 => KlineInterval.TwoHour,
			240 => KlineInterval.FourHour,
			360 => KlineInterval.SixHour,
			480 => KlineInterval.EightHour,
			720 => KlineInterval.TwelveHour,
			1440 => KlineInterval.OneDay,
			4320 => KlineInterval.ThreeDay,
			10080 => KlineInterval.OneWeek,
			43200 => KlineInterval.OneMonth,
			_ => throw new ArgumentException($"Unsupported interval: {interval}")
		};
	}

	public static KlineInterval ParseInterval(string interval)
	{
		return interval switch
		{
			"1m" => ToKlineInterval(TimeSpan.FromMinutes(1)),
			"3m" => ToKlineInterval(TimeSpan.FromMinutes(3)),
			"5m" => ToKlineInterval(TimeSpan.FromMinutes(5)),
			"15m" => ToKlineInterval(TimeSpan.FromMinutes(15)),
			"30m" => ToKlineInterval(TimeSpan.FromMinutes(30)),
			"1h" => ToKlineInterval(TimeSpan.FromHours(1)),
			"2h" => ToKlineInterval(TimeSpan.FromHours(2)),
			"4h" => ToKlineInterval(TimeSpan.FromHours(4)),
			"6h" => ToKlineInterval(TimeSpan.FromHours(6)),
			"8h" => ToKlineInterval(TimeSpan.FromHours(8)),
			"12h" => ToKlineInterval(TimeSpan.FromHours(12)),
			"1d" => ToKlineInterval(TimeSpan.FromDays(1)),
			"3d" => ToKlineInterval(TimeSpan.FromDays(3)),
			"1w" => ToKlineInterval(TimeSpan.FromDays(7)),
			"1M" => ToKlineInterval(TimeSpan.FromDays(30)),
			_ => throw new ArgumentException($"Unknown interval: {interval}")
		};
	}
}
