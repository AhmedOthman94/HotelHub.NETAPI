using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HotelHub.API.Models.Auth;
using HotelHub.API.Services.IServices;
using Microsoft.IdentityModel.Tokens;

namespace HotelHub.API.Services
{
	public class TokenService (IConfiguration configuration)
	: ITokenService
	{
		public string CreateAccessToken(ApplicationUser user, IList<string> roles)
		{
			var key = new SymmetricSecurityKey(
								Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));

			var credentials = new SigningCredentials(key, 
										SecurityAlgorithms.HmacSha256);

			var claims = new List<Claim>
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
				new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? ""),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Name, user.UserName ?? ""),
				new Claim(ClaimTypes.Email, user.Email ?? ""),

				new Claim("fisrtName", user.FirstName),
				new Claim("lastName", user.LastName)
			};

			foreach(var role in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role));
			}

			var token = new JwtSecurityToken
			(
				issuer: configuration["Jwt:Issuer"],
				audience: configuration["Jwt:Audience"],
				claims: claims,
				signingCredentials: credentials,
				expires: DateTime.UtcNow.AddMinutes(
								double.Parse(configuration["Jwt:AccessTokenExpirationInMinutes"]!))
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		public RefreshToken CreateRefreshToken(string ipAddress)
		{
			var randomBytes = new byte[64];
			using var rng = RandomNumberGenerator.Create();

			rng.GetBytes(randomBytes);

			return new RefreshToken
			{
				Token = Convert.ToBase64String(randomBytes),
				Expires = DateTime.UtcNow.AddDays(
								double.Parse(configuration["Jwt:RefreshTokenExpirationInDays"]!)),
				CreatedByIp = ipAddress,
				Created = DateTime.UtcNow
			};
		}
	}
}
