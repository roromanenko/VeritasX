using AutoMapper;
using Core.Domain;
using Infrastructure.Persistence.Entities;
using MongoDB.Bson;

namespace Infrastructure.Mapping.Profiles;

public class TradeProfile : Profile
{
	private static TradeSource ParseSource(string? source) =>
		Enum.TryParse<TradeSource>(source, out var src) ? src : TradeSource.Manual;

	public TradeProfile()
	{
		CreateMap<Trade, TradeDocument>()
			.ForMember(d => d.Id, o => o.MapFrom(s =>
				string.IsNullOrWhiteSpace(s.Id) ? ObjectId.GenerateNewId() : ObjectId.Parse(s.Id)))
			.ForMember(d => d.UserId, o => o.MapFrom(s =>
				string.IsNullOrWhiteSpace(s.UserId) ? ObjectId.Empty : ObjectId.Parse(s.UserId)))
			.ForMember(d => d.Source, o => o.MapFrom(s => s.Source.ToString()))
			.ForMember(d => d.BotId, o => o.MapFrom(s =>
				string.IsNullOrWhiteSpace(s.BotId) ? (ObjectId?)null : ObjectId.Parse(s.BotId)));

		CreateMap<TradeDocument, Trade>()
			.ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
			.ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId.ToString()))
			.ForMember(d => d.Source, o => o.MapFrom(s => ParseSource(s.Source)))
			.ForMember(d => d.BotId, o => o.MapFrom(s =>
				s.BotId.HasValue ? s.BotId.Value.ToString() : null));
	}
}
