using HotelHub.API.DTOs;

namespace HotelHub.API.Services.IServices
{
	public interface IHotelService
	{
		Task<IEnumerable<HotelDto>> GetAllHotelsAsync();

		Task<HotelDto?> GetHotelByIdAsync(Guid id);

		Task<HotelDto> CreateHotelAsync(CreateHotelDto dto);

		Task<bool> UpdateHotelAsync(Guid id, UpdateHotelDto dto);

		Task<bool> DeleteHotelByIdAsync(Guid id);

		Task<bool> HotelExistsAsync(Guid id);

		Task<bool> HotelExistsByNameAsync(string name);
	}
}
