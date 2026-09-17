namespace HotelHub.API.DTOs
{
	public class HotelDto
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public AddressDto Address { get; set; } = new();
		public decimal Rating { get; set; }
		public Guid CountryId { get; set; }
		public CountryDto? Country { get; set; }
	}
}
