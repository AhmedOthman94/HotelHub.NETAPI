using HotelHub.API.Enums;

namespace HotelHub.API.DTOs
{
	public class BookingDto
	{
		public Guid Id { get; set; }

		public Guid HotelId { get; set; }
		public string HotelName { get; set; } = string.Empty;

		public Guid UserId { get; set; }

		public DateOnly CheckIn { get; set; }
		public DateOnly CheckOut { get; set; }

		public int Guests { get; set; }

		public decimal TotalPrice { get; set; }

		public DateTime CreatedAtUtc { get; set; }
		public DateTime? UpdatedAtUtc { get; set; }

		public BookingStatus Status { get; set; }
	}
}
