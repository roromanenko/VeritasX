using Api.DTO;
using AutoMapper;
using Core.Domain.Statistics;

namespace Api.Mapping;

public class StatisticsProfile : Profile
{
	public StatisticsProfile()
	{
		CreateMap<EquityPoint, EquityPointDto>();
		CreateMap<BotStatistics, GetBotStatisticsResponse>();
		CreateMap<BotStatisticsSummary, BotStatisticsSummaryDto>();
		CreateMap<AccountStatistics, GetAccountStatisticsResponse>();
	}
}
