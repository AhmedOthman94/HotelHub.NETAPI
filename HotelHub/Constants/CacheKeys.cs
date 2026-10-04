namespace HotelHub.API.Constants
{
	public static class CacheKeys
	{
		public static string Country(Guid id)
			=> $"country:{id}";

		public static string Countries()
			=> "Countries";

		public static string Hotel(Guid id)
			=> $"hotel:{id}";
	}
}
