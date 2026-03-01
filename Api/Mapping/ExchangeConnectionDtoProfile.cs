using Api.DTO;
using AutoMapper;
using Core.Domain;

namespace Api.Mapping;

public class ExchangeConnectionDtoProfile : Profile
{
	public ExchangeConnectionDtoProfile()
	{
		CreateMap<AddExchangeConnectionRequest, ExchangeConnection>()
			.ConstructUsing(src => new ExchangeConnection
			{
				ApiKey = src.ApiKey,
				SecretKey = src.SecretKey,
				IsTestnet = src.IsTestnet
			});

		CreateMap<UpdateExchangeConnectionRequest, ExchangeConnection>()
			.ConstructUsing(src => new ExchangeConnection
			{
				ApiKey = src.ApiKey,
				SecretKey = src.SecretKey,
				IsTestnet = src.IsTestnet
			});

		CreateMap<ExchangeConnection, ExchangeConnectionResponse>()
			.ConstructUsing((src, ctx) => new ExchangeConnectionResponse(
				ctx.Items["Exchange"] is ExchangeName exchange ? exchange : throw new InvalidOperationException("Exchange not provided"),
				src.IsTestnet,
				src.CreatedAt,
				src.LastUsedAt
			));
	}
}
