using Api.DTO;
using AutoMapper;
using Core.Domain;

namespace Api.Mapping
{
	public class ExchangeDtoProfile : Profile
	{
		public ExchangeDtoProfile()
		{
			CreateMap<TradingPair, TradingPairDto>();

			CreateMap<Price, PriceDto>();

			CreateMap<Candle, CandleDto>()
				.ForMember(dest => dest.OpenTime, opt => opt.MapFrom(src => src.OpenTimeUtc));

			CreateMap<Portfolio, PortfolioDto>()
				.ForMember(dest => dest.Exchange, opt => opt.MapFrom(src => src.Exchange.ToString()));

			CreateMap<Balance, BalanceDto>();

			CreateMap<Order, OrderDto>()
				.ForMember(dest => dest.Side, opt => opt.MapFrom(src => src.Side.ToString()))
				.ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
				.ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

			CreateMap<PlaceOrderRequest, Order>()
				.ForMember(dest => dest.Side, opt => opt.MapFrom(src => Enum.Parse<OrderSide>(src.Side)))
				.ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<OrderType>(src.Type)))
				.ForMember(dest => dest.Id, opt => opt.Ignore())
				.ForMember(dest => dest.UserId, opt => opt.Ignore())
				.ForMember(dest => dest.ExchangeOrderId, opt => opt.Ignore())
				.ForMember(dest => dest.Exchange, opt => opt.Ignore())
				.ForMember(dest => dest.QuoteQuantity, opt => opt.Ignore())
				.ForMember(dest => dest.IsTestnet, opt => opt.Ignore())
				.ForMember(dest => dest.Status, opt => opt.Ignore())
				.ForMember(dest => dest.FilledQuantity, opt => opt.Ignore())
				.ForMember(dest => dest.AverageFillPrice, opt => opt.Ignore())
				.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
				.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
				.ForMember(dest => dest.ExecutedAt, opt => opt.Ignore());

			CreateMap<Trade, TradeDto>()
				.ForMember(dest => dest.Side, opt => opt.MapFrom(src => src.Side.ToString()));
		}
	}
}
