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
				.ForMember(d => d.Id, o => o.MapFrom(s =>
				string.IsNullOrWhiteSpace(s.Id) ? ObjectId.GenerateNewId() : ObjectId.Parse(s.Id)))
				.ForMember(d => d.JobId, o => o.MapFrom(s => ObjectId.Parse(s.JobId)))
				.ForMember(d => d.Candles, o => o.MapFrom(s => s.Candles));

			CreateMap<CandleChunkDocument, CandleChunk>()
				.ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
				.ForMember(d => d.JobId, o => o.MapFrom(s => s.JobId.ToString()))
				.ForMember(d => d.Candles, o => o.MapFrom(s => s.Candles));
		}
	}
}
