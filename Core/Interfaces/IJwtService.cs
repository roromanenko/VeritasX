using Core.Domain;

namespace Core.Interfaces;

public interface IJwtService
{
	string GenerateToken(User user);
}