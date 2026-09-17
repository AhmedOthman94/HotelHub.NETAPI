using HotelHub.API.Enums;

namespace HotelHub.API.DTOs
{
	public class RoomFilterDto
	{
		public RoomType? RoomType { get; set; }

		public int? MinCapacity { get; set; }

		public int? MaxCapacity { get; set; }

		public decimal? MinPrice { get; set; }

		public decimal? MaxPrice { get; set; }
	}
}