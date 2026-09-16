namespace HotelHub.API.DTOs
{
	public class CreateRoomDto
	{
		public string RoomNumber { get; set; } = string.Empty;
		public int Capacity { get; set; }
	}
}
