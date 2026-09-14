namespace HotelHub.API.DTOs
{
	public class CreateHotelDto
	{
		public string Name { get; set; } = string.Empty;
		public AddressDto Address { get; set; } = new();
		public decimal Rating { get; set; }
		public decimal PerNight { get; set; }
		public Guid CountryId { get; set; }
	}
}
