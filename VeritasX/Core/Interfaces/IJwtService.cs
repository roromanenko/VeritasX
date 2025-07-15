using System.Security.Claims;
using VeritasX.Infrastructure.Persistence.Entities;

namespace VeritasX.Core.Interfaces;

public interface IJwtService
{
	string GenerateToken(User user);
}