using AutoMapper;
using HotelHub.API.Data;
using HotelHub.API.DTOs;
using HotelHub.API.Entity;
using HotelHub.API.Enums;
using HotelHub.API.Extensions;
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

		public async Task<PagedResult<CountryDto>> GetAllCountriesAsync(
							string? searchTerm,
							SortingRequest? sorting,
							int pageNumber,
							int pageSize)
		{
			var query = context.Countries
				.AsNoTracking()
				.Search(
					searchTerm,
					c => c.Name,
					c => c.CountryCode);

			if (sorting is not null &&
				!string.IsNullOrWhiteSpace(sorting.SortBy))
			{
				query = query.OrderByProperty(
					sorting.SortBy,
					sorting.SortDirection == SortDirection.Descending);
			}
			else
			{
				query = query.OrderBy(c => c.Name);
			}

			var result = await query.ToPagedResultAsync(
				pageNumber,
				pageSize);

			return result.MapTo<Country, CountryDto>(mapper);
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
