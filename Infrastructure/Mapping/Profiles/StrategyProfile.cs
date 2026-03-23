using AutoMapper;
using Core.Domain;
using Infrastructure.Persistence.Entities;
using MongoDB.Bson;

namespace Infrastructure.Mapping.Profiles;

public class StrategyProfile : Profile
{
	public StrategyProfile()
	{
		CreateMap<Strategy, StrategyDocument>()
			.ForMember(d => d.Id, o => o.MapFrom(s =>
				string.IsNullOrWhiteSpace(s.Id) ? ObjectId.GenerateNewId() : ObjectId.Parse(s.Id)))
			.ForMember(d => d.AuthorId, o => o.MapFrom(s => ObjectId.Parse(s.AuthorId)));

		CreateMap<StrategyDocument, Strategy>()
			.ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
			.ForMember(d => d.AuthorId, o => o.MapFrom(s => s.AuthorId.ToString()));

		CreateMap<UserStrategyLibrary, UserStrategyLibraryDocument>()
			.ForMember(d => d.Id, o => o.MapFrom(s =>
				string.IsNullOrWhiteSpace(s.Id) ? ObjectId.GenerateNewId() : ObjectId.Parse(s.Id)))
			.ForMember(d => d.UserId, o => o.MapFrom(s => ObjectId.Parse(s.UserId)))
			.ForMember(d => d.StrategyId, o => o.MapFrom(s => ObjectId.Parse(s.StrategyId)));

		CreateMap<UserStrategyLibraryDocument, UserStrategyLibrary>()
			.ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
			.ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId.ToString()))
			.ForMember(d => d.StrategyId, o => o.MapFrom(s => s.StrategyId.ToString()));
	}
}
