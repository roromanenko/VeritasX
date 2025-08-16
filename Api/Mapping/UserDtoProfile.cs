using Api.DTO;
using AutoMapper;
using Core.Domain;
using System.Data;

namespace Api.Mapping;

public class UserDtoProfile : Profile
{
	public UserDtoProfile()
	{
		CreateMap<User, UserDto>().ReverseMap();
	}
}