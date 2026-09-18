using HotelHub.API.DTOs;
using HotelHub.API.Models;

namespace HotelHub.API.Services.IServices
{
	public interface IHotelService
	{
		Task<PagedResult<HotelDto>> GetAllHotelsAsync(
					string? searchTerm,
					HotelFilterDto? filter,
					SortingRequest? sorting,
					int pageNumber,
					int pageSize);

		Task<HotelDto?> GetHotelByIdAsync(Guid id);

		Task<HotelDto> CreateHotelAsync(CreateHotelDto dto);

		Task<bool> UpdateHotelAsync(Guid id, UpdateHotelDto dto);

		Task<bool> DeleteHotelByIdAsync(Guid id);

		Task<bool> HotelExistsAsync(Guid id);

		Task<bool> HotelExistsByNameAsync(string name);
	}
}
