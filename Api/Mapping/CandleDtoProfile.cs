using AutoMapper;
using Core.Domain;
using Api.DTO;

namespace Api.Mapping;

public class CandleDtoProfile : Profile
{
	public CandleDtoProfile()
	{
		CreateMap<Candle, CandleDto>()
			.ForMember(d => d.OpenTime, o => o.MapFrom(s => s.OpenTimeUtc.UtcDateTime));
	}
}