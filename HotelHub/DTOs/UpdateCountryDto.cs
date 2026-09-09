namespace HotelHub.API.DTOs
{
	public class UpdateCountryDto
	{
		public Guid Id	{ get; set; }
		public string Name { get; set; } = string.Empty;
		public string CountryCode { get; set; } = string.Empty;
	}
}
