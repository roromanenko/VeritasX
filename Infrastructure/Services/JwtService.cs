using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Core.Interfaces;
using Infrastructure.Persistence.Entities;
using Core.Options;
using Core.Domain;


namespace Infrastructure.Services;

public class JwtService : IJwtService
{
	private readonly JwtOptions _jwtOptions;

    public JwtService(IOptions<JwtOptions> jwtOptions)
	{
		_jwtOptions = jwtOptions.Value;
	}

	public string GenerateToken(User user)
	{
		ArgumentNullException.ThrowIfNull(user);
		if (string.IsNullOrWhiteSpace(user.Id)) throw new ArgumentNullException("User.Id is required.", nameof(user));
		if (string.IsNullOrWhiteSpace(user.Username)) throw new ArgumentNullException("User.Username is required.", nameof(user));

		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, user.Id),
			new(ClaimTypes.Name, user.Username),
			new(JwtRegisteredClaimNames.Sub, user.Id),
			new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		};

		if (user.Roles is not null && user.Roles.Count != 0)
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