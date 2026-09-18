using HotelHub.API.DTOs;
using HotelHub.API.Models;

namespace HotelHub.API.Services.IServices
{
	public interface ICountryService
	{
		Task<PagedResult<CountryDto>> GetAllCountriesAsync(
				string? searchTerm,
				SortingRequest? sorting,
				int pageNumber,
				int pageSize);

		Task<CountryDto?> GetCountryByIdAsync(Guid id);

		Task<CountryDto> CreateCountryAsync(CreateCountryDto dto);

		Task<bool> UpdateCountryAsync(Guid id, UpdateCountryDto dto);

		Task<bool> DeleteCountryByIdAsync(Guid id);

		Task<bool> CountryExistsAsync(Guid id);

		Task<bool> CountryExistsByNameAsync(string name);
	}
}