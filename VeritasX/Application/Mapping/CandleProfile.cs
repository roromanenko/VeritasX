using AutoMapper;
using VeritasX.Core.DTO;
using VeritasX.Core.Domain;

namespace VeritasX.Application.Mapping;

public class CandleProfile : Profile
{
	public CandleProfile()
	{
		CreateMap<Candle, CandleDto>();
	}
}