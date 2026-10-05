namespace HotelHub.API.Services.Caching
{
	public interface ICacheVersionService
	{
		Task<long> GetAsync(string key);
		Task<long> IncrementAsync(string key);
	}
}
