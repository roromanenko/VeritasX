using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.Domain;
using Core.Interfaces;
using Core.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;


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
		if (user is null) throw new ArgumentNullException(nameof(user), "User can`t be null");
		if (string.IsNullOrWhiteSpace(user.Id)) throw new ArgumentNullException(nameof(user), "User.Id is required.");
		if (string.IsNullOrWhiteSpace(user.Username)) throw new ArgumentNullException(nameof(user), "User.Username is required.");

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
