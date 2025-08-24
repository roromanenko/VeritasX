using Core.Domain;
using Core.Options;
using Infrastructure.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Tests

{
	public class JwtServiceTests
	{
		public static JwtOptions DefaultOptions() => new JwtOptions
		{
			Issuer = "TestIssuer",
			Audience = "TestAudience",
			SecretKey = "super_secret_key_which_is_long_enough_for_hmac_sha256",
			ExpirationHours = 1
		};

		private static JwtService CreateSut(JwtOptions? options = null)
		{
			return new JwtService(Options.Create(options ?? DefaultOptions()));
		}

		private static (ClaimsPrincipal principal, SecurityToken token) Validate(string jwt, JwtOptions options)
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey));

			var parameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = key,
				ValidateIssuer = true,
				ValidIssuer = options.Issuer,
				ValidateAudience = true,
				ValidAudience = options.Audience,
				ValidateLifetime = true,
				ClockSkew = TimeSpan.Zero
			};

			var principal = tokenHandler.ValidateToken(jwt, parameters, out var validatedToken);
			return (principal, validatedToken);
		}

		[Fact]
		public void GenerateToken_Throws_WhenUserIsNull()
		{
			var sut = CreateSut();
			Assert.Throws<ArgumentNullException>(() => sut.GenerateToken(null!));
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("  ")]
		public void GenerateToken_Trows_WhenIdIsMissing(string? badId)
		{
			var sut = CreateSut();
			var user = new User { Id = badId, Username = "euhene", PasswordHash = "some_password_hash" };
			var exeption = Assert.Throws<ArgumentNullException>(() => sut.GenerateToken(user));

			Assert.Equal("User.Id is required.", exeption.ParamName);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("  ")]
		public void GenerateToken_Trows_WhenUsernameIsMissing(string? badUsername)
		{
			var sut = CreateSut();
			var user = new User { Id = "GoodId", Username = badUsername, PasswordHash = "some_password_hash" };
			var exeption = Assert.Throws<ArgumentNullException>(() => sut.GenerateToken(user));

			Assert.Equal("User.Username is required.", exeption.ParamName);
		}

		[Fact]
		public void GenerateToken_ValidUser_ProducesSignedJwtWithExpectedClaims()
		{
			var opts = DefaultOptions();
			var sut = CreateSut();

			var user = new User
			{
				Id = "123",
				Username = "eugene",
				PasswordHash = "some_password_hash",
				Roles = new List<string> { "admin", "user" }
			};

			var jwt = sut.GenerateToken(user);

			var handler = new JwtSecurityTokenHandler();
			var token = handler.ReadJwtToken(jwt);

			Assert.Contains(token.Claims, c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id);
			Assert.Contains(token.Claims, c => c.Type == JwtRegisteredClaimNames.Jti && !string.IsNullOrEmpty(c.Value));

			var roleClaims = token.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
			Assert.Contains("admin", roleClaims);
			Assert.Contains("user", roleClaims);
		}

		[Fact]
		public void GenerateToken_WhenNoUserRoles_RoleClaimsMissing()
		{
			var opts = DefaultOptions();
			var sut = CreateSut();

			var user = new User
			{
				Id = "123",
				Username = "eugene",
				PasswordHash = "some_password_hash",
				Roles = new List<string>()
			};

			var jwt = sut.GenerateToken(user);

			var (principal, _) = Validate(jwt, opts);
			Assert.DoesNotContain(principal.Claims, c => c.Type == ClaimTypes.Role);
		}

		[Fact]
		public void GenerateToken_SetsExpiration_Issuer_Audience_Correctly()
		{
			var opts = DefaultOptions();
			opts.ExpirationHours = 2;

			var before = DateTime.UtcNow;
			var sut = CreateSut(opts);

			var user = new User
			{
				Id = "123",
				Username = "eugene",
				PasswordHash = "some_password_hash",
			};

			var jwt = sut.GenerateToken(user);
			var handler = new JwtSecurityTokenHandler();
			var token = handler.ReadJwtToken(jwt);

			Assert.Equal(opts.Issuer, token.Issuer);
			Assert.Contains(opts.Audience, token.Audiences);

			var expUtc = DateTimeOffset.FromUnixTimeSeconds(long.Parse(token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Exp).Value)).UtcDateTime;
			var expected = before.AddHours(opts.ExpirationHours);
			Assert.InRange(expUtc, expected.AddSeconds(-5), expected.AddSeconds(5));
		}
	}

	internal static class TestBoolExtensions
	{
		public static void ShouldBeTrue(this bool condition)
			=> Assert.True(condition);
	}
}