using Api.DTO;
using AutoMapper;
using Core.Domain;

namespace Api.Mapping;

public class BotDtoProfile : Profile
{
	public BotDtoProfile()
	{
		CreateMap<BotConfiguration, BotDto>();
		CreateMap<RiskParameters, RiskParametersDto>().ReverseMap();
		CreateMap<BotTradeRecord, BotTradeRecordDto>()
			.ForMember(d => d.Exchange, o => o.MapFrom(s => s.Exchange.ToString()));

		CreateMap<CreateBotRequest, BotConfiguration>()
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.UserId, opt => opt.Ignore())
			.ForMember(dest => dest.StrategySnapshot, opt => opt.Ignore())
			.ForMember(dest => dest.StrategyVersion, opt => opt.Ignore())
			.ForMember(dest => dest.Status, opt => opt.Ignore())
			.ForMember(dest => dest.ErrorMessage, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.StartedAt, opt => opt.Ignore())
			.ForMember(dest => dest.StoppedAt, opt => opt.Ignore());
	}
}
