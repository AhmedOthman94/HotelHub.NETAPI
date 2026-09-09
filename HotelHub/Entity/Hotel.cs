namespace HotelHub.API.Entity
{
	public class Hotel
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public Address Address { get; set; } = new();
		public decimal Rating { get; set; }

		//Foreign key
		public Guid CountryId { get; set; }
		//Navigation property
		public Country? Country { get; set; }
	}
}
