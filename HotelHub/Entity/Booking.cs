using HotelHub.API.Enums;
using HotelHub.API.Models.Auth;

namespace HotelHub.API.Entity
{
	public class Booking
	{
		public Guid Id { get; set; }

		// Fk to Room
		public Guid RoomId { get; set; }
		public Room Room { get; set; } = null!;

		// Fk to ApplicationUser
		public Guid UserId { get; set; }
		public ApplicationUser User { get; set; } = null!;

		public DateOnly CheckIn { get; set; }
		public DateOnly CheckOut { get; set; }

		public int Guests { get; set; }
		public decimal TotalPrice { get; set; }

		public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAtUtc { get; set; }

		public BookingStatus Status { get; set; } = BookingStatus.Pending;
	}
}
