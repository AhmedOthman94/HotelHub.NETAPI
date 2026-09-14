using HotelHub.API.Models.Auth;

namespace HotelHub.API.Entity
{
	public class HotelAdmin
	{
		public Guid Id { get; set; }

		// Fk to Hotel
		public Guid HotelId { get; set; }
		public Hotel Hotel { get; set; } = null!;

		// Fk to ApplicationUser
		public Guid UserId { get; set; }
		public ApplicationUser User { get; set; } = null!;
	}
}
