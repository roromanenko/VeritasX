using AutoMapper;
using Core.Domain;
using Infrastructure.Persistence.Entities;
using MongoDB.Bson;

namespace Infrastructure.Mapping.Profiles
{
	public class CandleChunkProfile : Profile
	{
		public CandleChunkProfile()
		{
			CreateMap<CandleChunk, CandleChunkDocument>()
				.ConstructUsing(s => new CandleChunkDocument
				{
					Id = ObjectId.Parse(s.Id),
					JobId = ObjectId.Parse(s.JobId),
					Symbol = s.Symbol,
					FromUtc = s.FromUtc.UtcDateTime,
					ToUtc = s.ToUtc.UtcDateTime,
					Interval = s.Interval,
					Candles = s.Candles.Select(c => new CandleDocument(
						c.OpenTimeUtc.UtcDateTime,
						c.Open, c.High, c.Low, c.Close, c.Volume)).ToList(),
					CreatedAt = s.CreatedAt.UtcDateTime
				});

			CreateMap<CandleChunkDocument, CandleChunk>()
				.ConstructUsing(d => new CandleChunk
				{
					Id = d.Id.ToString(),
					JobId = d.JobId.ToString(),
					Symbol = d.Symbol,
					FromUtc = new DateTimeOffset(DateTime.SpecifyKind(d.FromUtc, DateTimeKind.Utc)),
					ToUtc = new DateTimeOffset(DateTime.SpecifyKind(d.ToUtc, DateTimeKind.Utc)),
					Interval = d.Interval,
					Candles = d.Candles.Select(c => new Candle(
						new DateTimeOffset(DateTime.SpecifyKind(c.OpenTime, DateTimeKind.Utc)),
						c.Open, c.High, c.Low, c.Close, c.Volume)).ToList(),
					CreatedAt = new DateTimeOffset(DateTime.SpecifyKind(d.CreatedAt, DateTimeKind.Utc))
				});
		}
	}
}
