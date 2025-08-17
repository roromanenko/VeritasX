using AutoMapper;
using Infrastructure.Persistence.Entities;
using Core.Domain;
using MongoDB.Bson;

namespace Infrastructure.Mapping.Profiles;

public class DataCollectionJobProfile : Profile
{
	public DataCollectionJobProfile()
	{
		CreateMap<DataCollectionJob, DataCollectionJobDocument>()
			.ForMember(d => d.Id, opt => opt.MapFrom(s => ObjectId.Parse(s.Id)))
			.ForMember(d => d.UserId, opt => opt.MapFrom(s => ObjectId.Parse(s.UserId)));

		CreateMap<DataCollectionJobDocument, DataCollectionJob>()
			.ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
			.ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId.ToString()));
	}
}
