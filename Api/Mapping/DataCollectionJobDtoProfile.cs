using Api.DTO;
using AutoMapper;
using Core.Domain;
using Trading.Processors;
using Trading.Strategies;

namespace Api.Mapping;

public class DataCollectionJobDtoProfile : Profile
{
	public DataCollectionJobDtoProfile()
	{
		CreateMap<DataCollectionJob, DataCollectionJobDto>()
		.ForMember(d => d.FromUtc, o => o.MapFrom(s => s.FromUtc.UtcDateTime))
		.ForMember(d => d.ToUtc, o => o.MapFrom(s => s.ToUtc.UtcDateTime))
		.ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt.UtcDateTime))
		.ForMember(d => d.StartedAt, o => o.MapFrom(s => s.StartedAt.HasValue ? s.StartedAt.Value.UtcDateTime : (DateTime?)null))
		.ForMember(d => d.CompletedAt, o => o.MapFrom(s => s.CompletedAt.HasValue ? s.CompletedAt.Value.UtcDateTime : (DateTime?)null));

		CreateMap<TradingResult, TradingResultDto>().ReverseMap();
	}
}