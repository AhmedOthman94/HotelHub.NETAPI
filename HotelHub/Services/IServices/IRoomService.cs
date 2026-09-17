using HotelHub.API.DTOs;
using HotelHub.API.Models;

namespace HotelHub.API.Services.IServices
{
	public interface IRoomService
	{
		Task<PagedResult<RoomDto>> GetAllAsync(
			Guid hotelId,
			string? searchTerm,
			RoomFilterDto? filter,
			SortingRequest? sorting,
			int pageNumber,
			int pageSize);

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
