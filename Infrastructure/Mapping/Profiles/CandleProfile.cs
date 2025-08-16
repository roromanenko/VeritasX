using AutoMapper;
using Core.Domain;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Mapping.Profiles;

public class CandleProfile : Profile
{
	public CandleProfile()
	{
		CreateMap<Candle, CandleDocument>()
				.ConstructUsing(s => new CandleDocument(
					s.OpenTimeUtc,
					s.Open,
					s.High,
					s.Low,
					s.Close,
					s.Volume
				));

		CreateMap<CandleDocument, Candle>()
			.ConstructUsing(s => new Candle(
				s.OpenTime,
				s.Open,
				s.High,
				s.Low,
				s.Close,
				s.Volume
			));
	}
}