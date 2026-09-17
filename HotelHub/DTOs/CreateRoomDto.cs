using HotelHub.API.Enums;

namespace HotelHub.API.DTOs
{
	public class CreateRoomDto
	{
		public string RoomNumber { get; set; } = string.Empty;
		public RoomType RoomType { get; set; }
		public int Capacity { get; set; }
		public decimal PricePerNight { get; set; }
	}
}
