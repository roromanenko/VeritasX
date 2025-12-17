using AutoMapper;
using Binance.Net.Objects.Models;
using Binance.Net.Objects.Models.Spot;
using Core.Domain;
using Infrastructure.Exchanges.Binance.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Mapping.Profiles
{
	public class BinanceProfile : Profile
	{
		public BinanceProfile()
		{
			// BinancePlacedOrder -> Order (используется в PlaceOrder)
			CreateMap<BinancePlacedOrder, Order>()
				.ForMember(dest => dest.Id, opt => opt.Ignore())
				.ForMember(dest => dest.UserId, opt => opt.Ignore())
				.ForMember(dest => dest.ExchangeOrderId, opt => opt.MapFrom(src => src.Id.ToString()))
				.ForMember(dest => dest.Exchange, opt => opt.MapFrom(src => ExchangeName.Binance))
				.ForMember(dest => dest.Symbol, opt => opt.MapFrom(src => src.Symbol))
				.ForMember(dest => dest.Side, opt => opt.MapFrom(src => BinanceHelpers.ToBinanceSide(src.Side)))
				.ForMember(dest => dest.Type, opt => opt.MapFrom(src => BinanceHelpers.ToBinanceOrderType(src.Type)))
				.ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
				.ForMember(dest => dest.QuoteQuantity, opt => opt.MapFrom(src => src.QuoteQuantity))
				.ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
				.ForMember(dest => dest.IsTestnet, opt => opt.Ignore())
				.ForMember(dest => dest.Status, opt => opt.MapFrom(src => BinanceHelpers.ToBinanceStatus(src.Status)))
				.ForMember(dest => dest.FilledQuantity, opt => opt.MapFrom(src => src.QuantityFilled))
				.ForMember(dest => dest.AverageFillPrice, opt => opt.MapFrom(src =>
					src.QuantityFilled > 0 ? src.QuoteQuantityFilled / src.QuantityFilled : (decimal?)null))
				.ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src =>
					src.CreateTime != default ? new DateTimeOffset(src.CreateTime, TimeSpan.Zero) : DateTimeOffset.UtcNow))
				.ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src =>
					src.UpdateTime.HasValue && src.UpdateTime.Value != default
						? new DateTimeOffset(src.UpdateTime.Value, TimeSpan.Zero)
						: new DateTimeOffset(src.CreateTime, TimeSpan.Zero)))
				.ForMember(dest => dest.ExecutedAt, opt => opt.MapFrom(src =>
					src.Status == Binance.Net.Enums.OrderStatus.Filled && src.UpdateTime.HasValue && src.UpdateTime.Value != default
						? new DateTimeOffset(src.UpdateTime.Value, TimeSpan.Zero)
						: (DateTimeOffset?)null));

			// BinanceOrder -> Order (используется в GetOrder и CancelOrder)
			CreateMap<BinanceOrder, Order>()
				.ForMember(dest => dest.Id, opt => opt.Ignore())
				.ForMember(dest => dest.UserId, opt => opt.Ignore())
				.ForMember(dest => dest.ExchangeOrderId, opt => opt.MapFrom(src => src.Id.ToString()))
				.ForMember(dest => dest.Exchange, opt => opt.MapFrom(src => ExchangeName.Binance))
				.ForMember(dest => dest.Symbol, opt => opt.MapFrom(src => src.Symbol))
				.ForMember(dest => dest.Side, opt => opt.MapFrom(src => BinanceHelpers.ToBinanceSide(src.Side)))
				.ForMember(dest => dest.Type, opt => opt.MapFrom(src => BinanceHelpers.ToBinanceOrderType(src.Type)))
				.ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
				.ForMember(dest => dest.QuoteQuantity, opt => opt.MapFrom(src => src.QuoteQuantity))
				.ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
				.ForMember(dest => dest.IsTestnet, opt => opt.Ignore())
				.ForMember(dest => dest.Status, opt => opt.MapFrom(src => BinanceHelpers.ToBinanceStatus(src.Status)))
				.ForMember(dest => dest.FilledQuantity, opt => opt.MapFrom(src => src.QuantityFilled))
				.ForMember(dest => dest.AverageFillPrice, opt => opt.MapFrom(src =>
					src.QuantityFilled > 0 ? src.QuoteQuantityFilled / src.QuantityFilled : (decimal?)null))
				.ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src =>
					src.CreateTime != default ? new DateTimeOffset(src.CreateTime, TimeSpan.Zero) : DateTimeOffset.UtcNow))
				.ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src =>
					src.UpdateTime.HasValue && src.UpdateTime.Value != default
						? new DateTimeOffset(src.UpdateTime.Value, TimeSpan.Zero)
						: new DateTimeOffset(src.CreateTime, TimeSpan.Zero)))
				.ForMember(dest => dest.ExecutedAt, opt => opt.MapFrom(src =>
					src.Status == Binance.Net.Enums.OrderStatus.Filled && src.UpdateTime.HasValue && src.UpdateTime.Value != default
						? new DateTimeOffset(src.UpdateTime.Value, TimeSpan.Zero)
						: (DateTimeOffset?)null));

			// BinanceOrderBase -> Order (базовый класс, на всякий случай)
			CreateMap<BinanceOrderBase, Order>()
				.ForMember(dest => dest.Id, opt => opt.Ignore())
				.ForMember(dest => dest.UserId, opt => opt.Ignore())
				.ForMember(dest => dest.ExchangeOrderId, opt => opt.MapFrom(src => src.Id.ToString()))
				.ForMember(dest => dest.Exchange, opt => opt.MapFrom(src => ExchangeName.Binance))
				.ForMember(dest => dest.Symbol, opt => opt.MapFrom(src => src.Symbol))
				.ForMember(dest => dest.Side, opt => opt.MapFrom(src => BinanceHelpers.ToBinanceSide(src.Side)))
				.ForMember(dest => dest.Type, opt => opt.MapFrom(src => BinanceHelpers.ToBinanceOrderType(src.Type)))
				.ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
				.ForMember(dest => dest.QuoteQuantity, opt => opt.MapFrom(src => (decimal?)src.QuoteQuantity))
				.ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
				.ForMember(dest => dest.IsTestnet, opt => opt.Ignore())
				.ForMember(dest => dest.Status, opt => opt.MapFrom(src => BinanceHelpers.ToBinanceStatus(src.Status)))
				.ForMember(dest => dest.FilledQuantity, opt => opt.MapFrom(src => src.QuantityFilled))
				.ForMember(dest => dest.AverageFillPrice, opt => opt.MapFrom(src => (decimal?)null))
				.ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src =>
					src.CreateTime != default ? new DateTimeOffset(src.CreateTime, TimeSpan.Zero) : DateTimeOffset.UtcNow))
				.ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src =>
					src.UpdateTime.HasValue && src.UpdateTime.Value != default
						? new DateTimeOffset(src.UpdateTime.Value, TimeSpan.Zero)
						: new DateTimeOffset(src.CreateTime, TimeSpan.Zero)))
				.ForMember(dest => dest.ExecutedAt, opt => opt.MapFrom(src => (DateTimeOffset?)null));

			// BinanceTrade -> Trade
			CreateMap<BinanceTrade, Trade>()
				.ForMember(dest => dest.Id, opt => opt.Ignore())
				.ForMember(dest => dest.UserId, opt => opt.Ignore())
				.ForMember(dest => dest.Exchange, opt => opt.MapFrom(src => ExchangeName.Binance))
				.ForMember(dest => dest.ExchangeOrderId, opt => opt.MapFrom(src => src.OrderId.ToString()))
				.ForMember(dest => dest.ExchangeTradeId, opt => opt.MapFrom(src => src.Id.ToString()))
				.ForMember(dest => dest.IsTestnet, opt => opt.Ignore())
				.ForMember(dest => dest.Symbol, opt => opt.MapFrom(src => src.Symbol))
				.ForMember(dest => dest.Side, opt => opt.MapFrom(src => src.IsBuyer ? OrderSide.Buy : OrderSide.Sell))
				.ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
				.ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
				.ForMember(dest => dest.QuoteQuantity, opt => opt.MapFrom(src => src.QuoteQuantity))
				.ForMember(dest => dest.Fee, opt => opt.MapFrom(src => src.Fee))
				.ForMember(dest => dest.FeeAsset, opt => opt.MapFrom(src => src.FeeAsset))
				.ForMember(dest => dest.ExecutedAt, opt => opt.MapFrom(src =>
					src.Timestamp != default ? new DateTimeOffset(src.Timestamp, TimeSpan.Zero) : DateTimeOffset.UtcNow));
		}
	}
}
