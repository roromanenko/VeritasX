using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using VeritasX.Core.Interfaces;
using VeritasX.Infrastructure.Persistence.Entities;
using VeritasX.Core.Options;


namespace VeritasX.Application.Services;

public class JwtService : IJwtService
{
	private readonly JwtOptions _jwtOptions;

    public JwtService(IOptions<JwtOptions> jwtOptions)
	{
		_jwtOptions = jwtOptions.Value;
	}

	public string GenerateToken(User user)
	{
		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, user.Id.ToString()),
			new(ClaimTypes.Name, user.Username),
			new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
			new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		};

		if (user.Roles != null && user.Roles.Any())
		{
			claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
		}

		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
		var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var token = new JwtSecurityToken(
			issuer: _jwtOptions.Issuer,
			audience: _jwtOptions.Audience,
			claims: claims,
			expires: DateTime.UtcNow.AddHours(_jwtOptions.ExpirationHours),
			signingCredentials: credentials
		);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}
}