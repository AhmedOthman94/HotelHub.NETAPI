namespace HotelHub.API.DTOs
{
	public class UpdateRoomDto
	{
		public string RoomNumber { get; set; } = string.Empty;
		public int Capacity { get; set; }
	}
}
