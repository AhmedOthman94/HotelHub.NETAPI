namespace HotelHub.API.Entity
{
	public class Country
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string CountryCode { get; set; } = string.Empty;
		public ICollection<Hotel> Hotels { get; set; } = [];
	}
}
