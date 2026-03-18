using Api.DTO;
using AutoMapper;
using Core.Domain;

namespace Api.Mapping;

public class BotDtoProfile : Profile
{
	public BotDtoProfile()
	{
		CreateMap<BotConfiguration, BotDto>();
		CreateMap<StrategyDefinition, StrategyDefinitionDto>().ReverseMap();
		CreateMap<RiskParameters, RiskParametersDto>().ReverseMap();
		CreateMap<BotTradeRecord, BotTradeRecordDto>();

		CreateMap<CreateBotRequest, BotConfiguration>()
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.UserId, opt => opt.Ignore())
			.ForMember(dest => dest.Status, opt => opt.Ignore())
			.ForMember(dest => dest.ErrorMessage, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.StartedAt, opt => opt.Ignore())
			.ForMember(dest => dest.StoppedAt, opt => opt.Ignore());
	}
}
