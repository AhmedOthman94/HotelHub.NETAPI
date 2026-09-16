namespace HotelHub.API.DTOs
{
	public class RoomDto
	{
		public Guid Id { get; set; }
		public Guid HotelId { get; set; }
		public string RoomNumber { get; set; } = string.Empty;
		public int Capacity { get; set; }
	}
}
