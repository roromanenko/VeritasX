using Api.DTO;
using AutoMapper;
using Core.Domain;
using Core.Domain.Statistics;

namespace Api.Mapping;

public class StatisticsProfile : Profile
{
	public StatisticsProfile()
	{
		CreateMap<EquityPoint, EquityPointDto>();
		CreateMap<BotStatistics, GetBotStatisticsResponse>();
		CreateMap<BotStatisticsSummary, BotStatisticsSummaryDto>();
		CreateMap<ExchangeSummary, ExchangeSummaryDto>();
		CreateMap<AccountStatistics, GetAccountStatisticsResponse>();
		CreateMap<Balance, AssetPositionDto>();
		CreateMap<ExchangeSummary, ExchangePortfolioDto>()
			.ForMember(d => d.Assets, o => o.MapFrom(s => s.Assets));
	}
}
