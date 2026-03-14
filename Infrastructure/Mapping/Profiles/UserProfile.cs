using AutoMapper;
using Core.Domain;
using Infrastructure.Persistence.Entities;
using MongoDB.Bson;

namespace Infrastructure.Mapping.Profiles;

public class UserProfile : Profile
{
	public UserProfile()
	{
		CreateMap<User, UserDocument>()
			.ConstructUsing(u => new UserDocument
			{
				Id = ObjectId.Parse(u.Id),
				Username = u.Username,
				PasswordHash = u.PasswordHash,
				Roles = u.Roles.ToList()
			});

		CreateMap<UserDocument, User>()
			.ConstructUsing(e => new User
			{
				Id = e.Id.ToString(),
				Username = e.Username,
				PasswordHash = e.PasswordHash,
				Roles = e.Roles.ToList()
			})
			.ForMember(dest => dest.ExchangeConnections, opt => opt.Ignore());
	}
}
