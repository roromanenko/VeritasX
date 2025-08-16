using AutoMapper;
using Core.Domain;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Mapping.Profiles;

public class CandleProfile : Profile
{
	public CandleProfile()
	{
		CreateMap<Candle, CandleDocument>()
			.ConstructUsing(c => new CandleDocument(
				c.OpenTimeUtc.UtcDateTime,
				c.Open, c.High, c.Low, c.Close, c.Volume
				));

		CreateMap<CandleDocument, Candle>()
			.ConstructUsing(d => new Candle(
				new DateTimeOffset(DateTime.SpecifyKind(d.OpenTime, DateTimeKind.Utc)),
				d.Open, d.High, d.Low, d.Close, d.Volume
				));
	}
}