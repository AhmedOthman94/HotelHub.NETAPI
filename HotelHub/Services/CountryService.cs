using AutoMapper;
using HotelHub.API.Data;
using HotelHub.API.DTOs;
using HotelHub.API.Entity;
using HotelHub.API.Models;
using HotelHub.API.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace HotelHub.API.Services
{
	public class CountryService (ApplicationDbContext context,
									IMapper mapper
	)
	: ICountryService
	{
		public async Task<bool> CountryExistsAsync(Guid id)
		{
			return await context.Countries.AnyAsync( country => country.Id == id );
		}

		public async Task<bool> CountryExistsByNameAsync(string name)
		{
			return await context.Countries.AnyAsync(country => country.Name == name);
		}

		public async Task<CountryDto> CreateCountryAsync(CreateCountryDto dto)
		{
			var country = mapper.Map<Country>(dto);

			context.Countries.Add(country);
			await context.SaveChangesAsync();

			var countryDto = mapper.Map<CountryDto>(country);

			return countryDto;
		}

		public async Task<bool> DeleteCountryByIdAsync(Guid id)
		{
			var country = await context.Countries
										.FirstOrDefaultAsync(c => c.Id == id);
			if (country is null)
			{
				return false;
			}
			context.Countries.Remove(country);
			await context.SaveChangesAsync();

			return true;
		}

		public async Task<IEnumerable<CountryDto>> GetAllCountriesAsync()
		{
			var countries = await context.Countries
										.AsNoTracking()
										.OrderBy(c => c.Name)
										.ToListAsync();

			var countriesDto = mapper.Map<IEnumerable<CountryDto>>(countries);

			return countriesDto;
		}

		public async Task<CountryDto?> GetCountryByIdAsync(Guid id)
		{
			var country = await context.Countries
									.AsNoTracking()
									.FirstOrDefaultAsync(c => c.Id == id);
			if (country is null)
			{
				return null;
			}

			var countryDto = mapper.Map<CountryDto>(country);

			return countryDto;
		}

		public async Task<bool> UpdateCountryAsync(Guid id, UpdateCountryDto dto)
		{
			var country = await context.Countries
										.FirstOrDefaultAsync(c => c.Id == id);
			if (country is null)
			{
				return false;
			}

			mapper.Map(dto, country);
			await context.SaveChangesAsync();

			return true;
		}
	}
}
