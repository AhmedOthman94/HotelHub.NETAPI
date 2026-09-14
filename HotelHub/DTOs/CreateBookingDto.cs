namespace HotelHub.API.DTOs
{
	public class CreateBookingDto
	{
		public Guid HotelId { get; set; }

		public DateOnly CheckIn { get; set; }

		public DateOnly CheckOut { get; set; }

		public int Guests { get; set; }
	}
}
