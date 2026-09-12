using HotelHub.API.Models.Auth;

namespace HotelHub.API.Services.IServices
{
	public interface ITokenService
	{
		string CreateAccessToken(ApplicationUser user, IList<string> roles);
		RefreshToken CreateRefreshToken(string ipAddress);
	}
}
