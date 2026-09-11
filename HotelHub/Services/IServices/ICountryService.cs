using HotelHub.API.DTOs;

namespace HotelHub.API.Services.IServices
{
	public interface ICountryService
	{
		Task<IEnumerable<CountryDto>> GetAllCountriesAsync();

		Task<CountryDto?> GetCountryByIdAsync(Guid id);

		Task<CountryDto> CreateCountryAsync(CreateCountryDto dto);

		Task<bool> UpdateCountryAsync(Guid id, UpdateCountryDto dto);

		Task<bool> DeleteCountryByIdAsync(Guid id);

		Task<bool> CountryExistsAsync(Guid id);

		Task<bool> CountryExistsByNameAsync(string name);
	}
}