using HotelHub.API.Enums;

namespace HotelHub.API.DTOs
{
	public class BookingFilterDto
	{
		public BookingStatus? Status { get; set; }

		public DateOnly? CheckInFrom { get; set; }

		public DateOnly? CheckInTo { get; set; }

		public DateOnly? CheckOutFrom { get; set; }

		public DateOnly? CheckOutTo { get; set; }

		public int? MinGuests { get; set; }

		public int? MaxGuests { get; set; }
	}
}
