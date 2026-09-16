using HotelHub.API.DTOs;

namespace HotelHub.API.Services.IServices
{
	public interface IRoomService
	{
		Task<IEnumerable<RoomDto>> GetAllAsync(
			Guid hotelId);

		Task<RoomDto?> GetByIdAsync(
			Guid hotelId,
			Guid roomId);

		Task<RoomDto> CreateAsync(
			Guid hotelId,
			CreateRoomDto dto);

		Task<bool> UpdateAsync(
			Guid hotelId,
			Guid roomId,
			UpdateRoomDto dto);

		Task<bool> DeleteAsync(
			Guid hotelId,
			Guid roomId);
	}
}
