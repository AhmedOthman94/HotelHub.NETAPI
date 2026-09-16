using System.Collections.ObjectModel;

namespace HotelHub.API.Entity
{
	public class Hotel
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public Address Address { get; set; } = new();
		public decimal Rating { get; set; }
		public decimal PerNight { get; set; }

		//Foreign key
		public Guid CountryId { get; set; }
		//Navigation property
		public Country Country { get; set; } = null!;

		public ICollection<HotelAdmin> HotelAdmins { get; set; } = [];

		public ICollection<Booking> Bookings { get; set; } = [];

		public Collection<Room> Rooms { get; set; } = [];
	}
}
