namespace HotelHub.API.DTOs
{
	public class HotelFilterDto
	{
		public Guid? CountryId { get; set; }

		public decimal? MinRating { get; set; }

		public decimal? MaxRating { get; set; }
	}
}
