using Api.DTO;
using AutoMapper;
using Core.Domain;
using System.Data;

namespace Api.Mapping;

public class UserDtoProfile : Profile
{
	public UserDtoProfile()
	{
		CreateMap<User, UserDto>()
			.ConstructUsing(src => new UserDto(
				src.Id,
				src.Username,
				src.Roles,
				src.ExchangeConnections != null
					? src.ExchangeConnections.Keys.Select(k => k.ToString())
					: Enumerable.Empty<string>()
			));

		CreateMap<UserDto, User>()
			.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
			.ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
			.ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles.ToList()))
			.ForMember(dest => dest.ExchangeConnections, opt => opt.Ignore())
			.ForMember(dest => dest.PasswordHash, opt => opt.Ignore());
	}
}