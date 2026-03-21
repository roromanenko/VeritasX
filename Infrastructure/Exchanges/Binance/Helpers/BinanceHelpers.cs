using System.Globalization;
using Binance.Net.Enums;
using Binance.Net.Objects.Models.Spot;
using Core.Domain;
using BinanceNet = Binance.Net;

namespace Infrastructure.Exchanges.Binance.Helpers;

public static class BinanceHelpers
{
	#region Order side conversions

	/// <summary>
	/// Converts a Binance order side to the domain <see cref="Core.Domain.OrderSide"/>.
	/// </summary>
	/// <param name="side">Binance order side enum value.</param>
	/// <returns>Corresponding <see cref="Core.Domain.OrderSide"/></returns>
	/// <exception cref="ArgumentException">Thrown when the Binance order side is not supported.</exception>
	public static Core.Domain.OrderSide ToDomainSide(BinanceNet.Enums.OrderSide side) =>
		side switch
		{
			BinanceNet.Enums.OrderSide.Buy => Core.Domain.OrderSide.Buy,
			BinanceNet.Enums.OrderSide.Sell => Core.Domain.OrderSide.Sell,
			_ => throw new ArgumentException($"Unknown Binance order side: {side}")
		};

	/// <summary>
	/// Converts a domain <see cref="Core.Domain.OrderSide"/> to the Binance order side.
	/// </summary>
	/// <param name="side">Domain order side enum value.</param>
	/// <returns>Corresponding <see cref="BinanceNet.Enums.OrderSide"/>.</returns>
	/// <exception cref="ArgumentException">Thrown when the domain order side is not supported.</exception>
	public static BinanceNet.Enums.OrderSide ToBinanceSide(Core.Domain.OrderSide side) =>
		side switch
		{
			Core.Domain.OrderSide.Buy => BinanceNet.Enums.OrderSide.Buy,
			Core.Domain.OrderSide.Sell => BinanceNet.Enums.OrderSide.Sell,
			_ => throw new ArgumentException($"Unknown order side: {side}")
		};

	#endregion

	#region Order type conversions

	/// <summary>
	/// Converts a Binance order type to the domain <see cref="Core.Domain.OrderType"/>.
	/// </summary>
	/// <param name="type">Binance spot order type enum value.</param>
	/// <returns>Corresponding domain order type.</returns>
	/// <exception cref="ArgumentException">Thrown when the Binance order type is not supported.</exception>
	public static Core.Domain.OrderType ToDomainOrderType(SpotOrderType type) =>
		type switch
		{
			SpotOrderType.Market => OrderType.Market,
			SpotOrderType.Limit => OrderType.Limit,
			SpotOrderType.StopLoss => OrderType.StopLoss,
			SpotOrderType.StopLossLimit => OrderType.StopLimit,
			_ => throw new ArgumentException($"Unknown Binance order type: {type}")
		};

	/// <summary>
	/// Converts a domain <see cref="Core.Domain.OrderType"/> to the Binance order type.
	/// </summary>
	/// <param name="type">Domain order type enum value.</param>
	/// <returns>Corresponding Binance order type.</returns>
	/// <exception cref="ArgumentException">Thrown when the domain order type is not supported.</exception>
	public static SpotOrderType ToBinanceOrderType(Core.Domain.OrderType type) =>
		type switch
		{
			OrderType.Market => SpotOrderType.Market,
			OrderType.Limit => SpotOrderType.Limit,
			OrderType.StopLoss => SpotOrderType.StopLoss,
			OrderType.StopLimit => SpotOrderType.StopLossLimit,
			_ => throw new ArgumentException($"Unknown order type: {type}")
		};

	#endregion

	#region Order status conversions

	/// <summary>
	/// Converts a Binance order status to the domain <see cref="Core.Domain.OrderStatus"/>.
	/// </summary>
	/// <remarks>
	/// <see cref="BinanceNet.Enums.OrderStatus.PendingCancel"/> and
	/// <see cref="BinanceNet.Enums.OrderStatus.Expired"/> are both mapped to
	/// <see cref="Core.Domain.OrderStatus.Canceled"/>.
	/// </remarks>
	/// <param name="status">Binance order status enum value.</param>
	/// <returns>Corresponding domain order status.</returns>
	/// <exception cref="ArgumentException">Thrown when the Binance order status is not supported.</exception>
	public static Core.Domain.OrderStatus ToDomainStatus(BinanceNet.Enums.OrderStatus status) =>
		status switch
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

	#endregion

	#region Symbol filter helpers

	/// <summary>
	/// Gets the minimum allowed quantity for the trading pair.
	/// Returns <c>0</c> if the lot size filter is not defined.
	/// </summary>
	public static decimal GetMinQuantity(BinanceSymbol symbol) =>
		GetLotSizeFilter(symbol)?.MinQuantity ?? 0m;

	/// <summary>
	/// Gets the maximum allowed quantity for the trading pair.
	/// Returns <see cref="decimal.MaxValue"/> if the lot size filter is not defined.
	/// </summary>
	public static decimal GetMaxQuantity(BinanceSymbol symbol) =>
		GetLotSizeFilter(symbol)?.MaxQuantity ?? decimal.MaxValue;

	/// <summary>
	/// Gets the quantity step size (increment) for the trading pair.
	/// Returns <c>0</c> if the lot size filter is not defined.
	/// </summary>
	public static decimal GetStepSize(BinanceSymbol symbol) =>
		GetLotSizeFilter(symbol)?.StepSize ?? 0m;

	/// <summary>
	/// Gets the minimum allowed price for the trading pair.
	/// Returns <c>0</c> if the price filter is not defined.
	/// </summary>
	public static decimal GetMinPrice(BinanceSymbol symbol) =>
		GetPriceFilter(symbol)?.MinPrice ?? 0m;

	/// Gets the maximum allowed price for the trading pair.
	/// Returns <see cref="decimal.MaxValue"/> if the price filter is not defined.
	/// </summary>
	public static decimal GetMaxPrice(BinanceSymbol symbol) =>
		GetPriceFilter(symbol)?.MaxPrice ?? decimal.MaxValue;

	/// <summary>
	/// Gets the price tick size (increment) for the trading pair.
	/// Returns <c>0</c> if the price filter is not defined.
	/// </summary>
	public static decimal GetTickSize(BinanceSymbol symbol) =>
		GetPriceFilter(symbol)?.TickSize ?? 0m;

	/// <summary>
	/// Gets the minimum notional value (price × quantity) for the trading pair.
	/// Returns <c>0</c> if the notional filter is not defined, or if
	/// <paramref name="isMarket"/> is <see langword="true"/> and the filter does not apply to market orders.
	/// </summary>
	/// <param name="symbol">Binance symbol with exchange filters.</param>
	/// <param name="isMarket">
	/// When <see langword="true"/>, checks whether the notional filter applies to market orders.
	/// </param>
	public static decimal GetMinNotional(BinanceSymbol symbol, bool isMarket = false)
	{
		var notional = symbol.MinNotionalFilter;
		if (notional == null)
			return 0m;

		if (isMarket && notional.ApplyToMarketOrders == false)
			return 0m;

		return notional.MinNotional;
	}

	#endregion

	#region Precision helpers

	/// <summary>
	/// Calculates decimal precision (number of digits after decimal point) from step size.
	/// </summary>
	private static int GetPrecision(decimal stepSize)
	{
		var str = stepSize.ToString(CultureInfo.InvariantCulture);
		var decimalIndex = str.IndexOf('.');
		if (decimalIndex == -1)
			return 0;

		return str[(decimalIndex + 1)..].TrimEnd('0').Length;
	}

	#endregion

	#region Quantity / Price rounding

	/// <summary>
	/// Rounds quantity down to the nearest valid value according to step size.
	/// Returns <paramref name="quantity"/> unchanged if <paramref name="stepSize"/> is <c>0</c>.
	/// </summary>
	/// <param name="quantity">Raw quantity to round.</param>
	/// <param name="stepSize">Lot size step from the exchange filter.</param>
	public static decimal RoundQuantity(decimal quantity, decimal stepSize)
	{
		if (stepSize == 0) return quantity;

		var precision = GetPrecision(stepSize);
		return Math.Round(Math.Floor(quantity / stepSize) * stepSize, precision);
	}

	/// <summary>
	/// Rounds price down to the nearest valid value according to tick size.
	/// Returns <paramref name="price"/> unchanged if <paramref name="tickSize"/> is <c>0</c>.
	/// </summary>
	/// <param name="price">Raw price to round.</param>
	/// <param name="tickSize">Price tick size from the exchange filter.</param>
	public static decimal RoundPrice(decimal price, decimal tickSize)
	{
		if (tickSize == 0) return price;

		var precision = GetPrecision(tickSize);
		return Math.Round(Math.Floor(price / tickSize) * tickSize, precision);
	}

	#endregion

	#region Validation

	/// <summary>
	/// Validates that <paramref name="quantity"/> is within the allowed range
	/// and aligned to the lot size step for the given symbol.
	/// </summary>
	/// <param name="quantity">Quantity to validate.</param>
	/// <param name="symbol">Binance symbol with exchange filters.</param>
	/// <returns>
	/// <see langword="true"/> if the quantity is valid; otherwise <see langword="false"/>.
	/// </returns>
	public static bool ValidateQuantity(decimal quantity, BinanceSymbol symbol)
	{
		var stepSize = GetStepSize(symbol);
		return IsInRange(quantity, GetMinQuantity(symbol), GetMaxQuantity(symbol))
			&& IsAlignedToStep(quantity, stepSize, RoundQuantity(quantity, stepSize));
	}

	/// <summary>
	/// Validates that <paramref name="price"/> is within the allowed range
	/// and aligned to the tick size for the given symbol.
	/// </summary>
	/// <param name="price">Price to validate.</param>
	/// <param name="symbol">Binance symbol with exchange filters.</param>
	/// <returns>
	/// <see langword="true"/> if the price is valid; otherwise <see langword="false"/>.
	/// </returns>
	public static bool ValidatePrice(decimal price, BinanceSymbol symbol)
	{
		var tickSize = GetTickSize(symbol);
		return IsInRange(price, GetMinPrice(symbol), GetMaxPrice(symbol))
			&& IsAlignedToStep(price, tickSize, RoundPrice(price, tickSize));
	}

	/// <summary>
	/// Validates that the order notional value (price × quantity) meets
	/// the minimum notional requirement for the given symbol.
	/// </summary>
	/// <param name="price">Order price.</param>
	/// <param name="quantity">Order quantity.</param>
	/// <param name="symbol">Binance symbol with exchange filters.</param>
	/// <param name="isMarket">
	/// When <see langword="true"/>, checks whether the notional filter applies to market orders.
	/// </param>
	/// <returns>
	/// <see langword="true"/> if the notional value meets the minimum; otherwise <see langword="false"/>.
	/// </returns>
	public static bool ValidateNotional(decimal price, decimal quantity, BinanceSymbol symbol, bool isMarket = false) =>
		price * quantity >= GetMinNotional(symbol, isMarket);

	#endregion

	#region Kline interval conversions

	/// <summary>
	/// Converts a <see cref="TimeSpan"/> to the corresponding Binance <see cref="KlineInterval"/>.
	/// </summary>
	/// <param name="interval">Time span representing the candle interval.</param>
	/// <returns>Corresponding Binance kline interval.</returns>
	/// <exception cref="ArgumentException">Thrown when the time span does not map to a supported Binance interval.</exception>
	public static KlineInterval ToKlineInterval(TimeSpan interval) =>
		interval.TotalMinutes switch
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

	/// <summary>
	/// Parses a Binance interval string (e.g. <c>"1m"</c>, <c>"4h"</c>, <c>"1d"</c>)
	/// into the corresponding <see cref="KlineInterval"/>.
	/// </summary>
	/// <param name="interval">Binance interval string.</param>
	/// <returns>Corresponding Binance kline interval.</returns>
	/// <exception cref="ArgumentException">Thrown when the interval string is not recognised.</exception>
	public static KlineInterval ParseInterval(string interval) =>
		interval switch
		{
			"1m" => KlineInterval.OneMinute,
			"3m" => KlineInterval.ThreeMinutes,
			"5m" => KlineInterval.FiveMinutes,
			"15m" => KlineInterval.FifteenMinutes,
			"30m" => KlineInterval.ThirtyMinutes,
			"1h" => KlineInterval.OneHour,
			"2h" => KlineInterval.TwoHour,
			"4h" => KlineInterval.FourHour,
			"6h" => KlineInterval.SixHour,
			"8h" => KlineInterval.EightHour,
			"12h" => KlineInterval.TwelveHour,
			"1d" => KlineInterval.OneDay,
			"3d" => KlineInterval.ThreeDay,
			"1w" => KlineInterval.OneWeek,
			"1M" => KlineInterval.OneMonth,
			_ => throw new ArgumentException($"Unknown interval: {interval}")
		};

	#endregion

	#region Private helpers

	private static BinanceSymbolLotSizeFilter? GetLotSizeFilter(BinanceSymbol symbol) =>
		symbol.LotSizeFilter;

	private static BinanceSymbolPriceFilter? GetPriceFilter(BinanceSymbol symbol) =>
		symbol.PriceFilter;

	/// <summary>
	/// Returns true if <paramref name="value"/> is within [min, max].
	/// </summary>
	private static bool IsInRange(decimal value, decimal min, decimal max) =>
		value >= min && value <= max;

	/// <summary>
	/// Returns true if <paramref name="value"/> is already aligned to the given step,
	/// i.e. rounding down produces the same value.
	/// </summary>
	private static bool IsAlignedToStep(decimal value, decimal step, decimal rounded)
	{
		if (step == 0) return true;

		var precision = GetPrecision(step);
		return Math.Round(value, precision) == rounded;
	}

	#endregion
}
