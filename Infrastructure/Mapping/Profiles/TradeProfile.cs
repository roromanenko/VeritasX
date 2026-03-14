using AutoMapper;
using Core.Domain;
using Infrastructure.Persistence.Entities;
using MongoDB.Bson;

namespace Infrastructure.Mapping.Profiles;

public class TradeProfile : Profile
{
	public TradeProfile()
	{
		CreateMap<Trade, TradeDocument>()
			.ForMember(d => d.Id, o => o.MapFrom(s =>
				string.IsNullOrWhiteSpace(s.Id) ? ObjectId.GenerateNewId() : ObjectId.Parse(s.Id)))
			.ForMember(d => d.UserId, o => o.MapFrom(s =>
				string.IsNullOrWhiteSpace(s.UserId) ? ObjectId.Empty : ObjectId.Parse(s.UserId)));

		CreateMap<TradeDocument, Trade>()
			.ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
			.ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId.ToString()));
	}
}
