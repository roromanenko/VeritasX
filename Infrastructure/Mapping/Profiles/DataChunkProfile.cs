using AutoMapper;
using Core.Domain;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Mapping.Profiles
{
	public class DataChunkProfile : Profile
	{
		public DataChunkProfile()
		{
			CreateMap<DataChunk, DataChunkDocument>()
				.ForMember(d => d.FromUtc, m => m.MapFrom(s => s.FromUtc.UtcDateTime))
				.ForMember(d => d.ToUtc, m => m.MapFrom(s => s.ToUtc.UtcDateTime));

			CreateMap<DataChunkDocument, DataChunk>()
				.ConstructUsing(d => new DataChunk
				{
					FromUtc = new DateTimeOffset(DateTime.SpecifyKind(d.FromUtc, DateTimeKind.Utc)),
					ToUtc = new DateTimeOffset(DateTime.SpecifyKind(d.ToUtc, DateTimeKind.Utc)),
					State = d.State,
					RetryCount = d.RetryCount,
					ErrorMessage = d.ErrorMessage
				});
		}
	}
}
