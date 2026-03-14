using AutoMapper;
using Core.Domain;
using Infrastructure.Persistence.Entities;
using MongoDB.Bson;

namespace Infrastructure.Mapping.Profiles;

public class BotProfile : Profile
{
	public BotProfile()
	{
		CreateMap<BotConfiguration, BotConfigurationDocument>()
			.ForMember(d => d.Id, o => o.MapFrom(s =>
				string.IsNullOrWhiteSpace(s.Id) ? ObjectId.GenerateNewId() : ObjectId.Parse(s.Id)))
			.ForMember(d => d.UserId, o => o.MapFrom(s => ObjectId.Parse(s.UserId)));

		CreateMap<BotConfigurationDocument, BotConfiguration>()
			.ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
			.ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId.ToString()));

		CreateMap<StrategyDefinition, StrategyDefinitionDocument>().ReverseMap();
		CreateMap<RiskParameters, RiskParametersDocument>().ReverseMap();

		CreateMap<BotTradeRecord, BotTradeRecordDocument>()
			.ForMember(d => d.Id, o => o.MapFrom(s =>
				string.IsNullOrWhiteSpace(s.Id) ? ObjectId.GenerateNewId() : ObjectId.Parse(s.Id)))
			.ForMember(d => d.BotId, o => o.MapFrom(s => ObjectId.Parse(s.BotId)))
			.ForMember(d => d.UserId, o => o.MapFrom(s => ObjectId.Parse(s.UserId)))
			.ForMember(d => d.TradeId, o => o.MapFrom(s =>
				string.IsNullOrWhiteSpace(s.TradeId) ? (ObjectId?)null : ObjectId.Parse(s.TradeId)));

		CreateMap<BotTradeRecordDocument, BotTradeRecord>()
			.ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
			.ForMember(d => d.BotId, o => o.MapFrom(s => s.BotId.ToString()))
			.ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId.ToString()))
			.ForMember(d => d.TradeId, o => o.MapFrom(s =>
				s.TradeId.HasValue ? s.TradeId.Value.ToString() : null));
	}
}
