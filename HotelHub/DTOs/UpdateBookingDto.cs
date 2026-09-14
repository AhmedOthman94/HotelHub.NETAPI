namespace HotelHub.API.DTOs
{
	public class UpdateBookingDto
	{
		public Guid Id { get; set; }

		public DateOnly CheckIn { get; set; }

		public DateOnly CheckOut { get; set; }

		public int Guests { get; set; }
	}
}
