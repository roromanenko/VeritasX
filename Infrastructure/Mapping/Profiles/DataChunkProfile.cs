using AutoMapper;
using Core.Domain;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Mapping.Profiles
{
	public class DataChunkProfile : Profile
	{
		public DataChunkProfile()
		{
			CreateMap<DataChunk, DataChunkDocument>();

			CreateMap<DataChunkDocument, DataChunk>();
		}
	}
}
