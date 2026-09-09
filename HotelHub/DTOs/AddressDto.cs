namespace HotelHub.API.DTOs
{
	public class AddressDto
	{
		public string Street { get; set; } = string.Empty;
		public string City { get; set; } = string.Empty;
		public string? State { get; set; }
		public string? PostalCode { get; set; }
		public decimal Latitude { get; set; }
		public decimal Longitude { get; set; }
	}
}
