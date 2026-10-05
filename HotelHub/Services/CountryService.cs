using AutoMapper;
using HotelHub.API.Constants;
using HotelHub.API.Data;
using HotelHub.API.DTOs;
using HotelHub.API.Entity;
using HotelHub.API.Enums;
using HotelHub.API.Extensions;
using HotelHub.API.Models;
using HotelHub.API.Services.Caching;
using HotelHub.API.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace HotelHub.API.Services
{
	public class CountryService (ApplicationDbContext context,
									IMapper mapper,
									ICacheService cache,
									ICacheVersionService cacheVersion
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

			await cacheVersion.IncrementAsync(
					CacheKeys.CountriesVersion());

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

			await cache.RemoveAsync(
					CacheKeys.Country(id)
			);

			await cacheVersion.IncrementAsync(
					CacheKeys.CountriesVersion());

			return true;
		}

		public async Task<PagedResult<CountryDto>> GetAllCountriesAsync(
				string? searchTerm,
				SortingRequest? sorting,
				int pageNumber,
				int pageSize)
		{
			var version = await cacheVersion.GetAsync(
				CacheKeys.CountriesVersion()
			);

			var cacheKey = CacheKeys.CountriesList(
				version,
				searchTerm,
				sorting,
				pageNumber,
				pageSize);

			var cachedResult =
				await cache.GetAsync<PagedResult<CountryDto>>(
					cacheKey);

			if (cachedResult is not null)
			{
				return cachedResult;
			}

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

			var mappedResult =
				result.MapTo<Country, CountryDto>(mapper);

			await cache.SetAsync(
				cacheKey,
				mappedResult,
				TimeSpan.FromMinutes(5));

			return mappedResult;
		}

		public async Task<CountryDto?> GetCountryByIdAsync(Guid id)
		{
			var cacheKey = CacheKeys.Country(id);

			var cachedCountry = await cache.GetAsync<CountryDto>(cacheKey);

			if (cachedCountry is not null)
			{
				return cachedCountry;
			}

			var country = await context.Countries
									.AsNoTracking()
									.FirstOrDefaultAsync(c => c.Id == id);
			if (country is null)
			{
				return null;
			}
			var countryDto = mapper.Map<CountryDto>(country);

			await cache.SetAsync(cacheKey,
				countryDto,
				TimeSpan.FromMinutes(30));

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

			await cache.RemoveAsync(
					CacheKeys.Country(id)
			);

			await cacheVersion.IncrementAsync(
					CacheKeys.CountriesVersion());

			return true;
		}
	}
}
