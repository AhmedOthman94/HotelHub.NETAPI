using HotelHub.API.Enums;

namespace HotelHub.API.Entity
{
	public class Room
	{
		public Guid Id { get; set; }

		public Guid HotelId { get; set; }
		public Hotel Hotel { get; set; } = null!;

		public string RoomNumber { get; set; } = string.Empty;
		public RoomType RoomType { get; set; }
		public int Capacity { get; set; }
		public decimal PricePerNight { get; set; }
	}
}
