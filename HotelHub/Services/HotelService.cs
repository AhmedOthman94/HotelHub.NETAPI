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
	public class HotelService(ApplicationDbContext context,
								IMapper mapper
	)
	: IHotelService
	{
		public async Task<bool> HotelExistsAsync(Guid id)
		{
			return await context.Hotels
								.AnyAsync(hotel => hotel.Id == id);
		}

		public async Task<bool> HotelExistsByNameAsync(string name)
		{
			return await context.Hotels
								.AnyAsync(hotel => hotel.Name == name);
		}

		public async Task<PagedResult<HotelDto>> GetAllHotelsAsync(
	string? searchTerm,
	HotelFilterDto? filter,
	SortingRequest? sorting,
	int pageNumber,
	int pageSize)
		{
			var query = context.Hotels
				.AsNoTracking()
				.Include(h => h.Country)
				.Search(
					searchTerm,
					h => h.Name);

			if (filter is not null)
			{
				query = query
					.WhereIf(
						filter.CountryId.HasValue,
						h => h.CountryId == filter.CountryId!.Value)

					.WhereIf(
						filter.MinRating.HasValue,
						h => h.Rating >= filter.MinRating!.Value)

					.WhereIf(
						filter.MaxRating.HasValue,
						h => h.Rating <= filter.MaxRating!.Value);
			}

			if (sorting is not null &&
				!string.IsNullOrWhiteSpace(sorting.SortBy))
			{
				query = query.OrderByProperty(
					sorting.SortBy,
					sorting.SortDirection == SortDirection.Descending);
			}
			else
			{
				query = query.OrderBy(h => h.Name);
			}

			var result = await query.ToPagedResultAsync(
				pageNumber,
				pageSize);

			return result.MapTo<Hotel, HotelDto>(mapper);
		}

		public async Task<HotelDto?> GetHotelByIdAsync(Guid id)
		{
			var hotel = await context.Hotels
									  .AsNoTracking()
									  .Include(h => h.Country)
									  .FirstOrDefaultAsync(h => h.Id == id);

			if (hotel is null)
			{
				return null;
			}

			var hotelDto = mapper.Map<HotelDto>(hotel);

			return hotelDto;
		}

		public async Task<HotelDto> CreateHotelAsync(CreateHotelDto dto)
		{
			var hotel = mapper.Map<Hotel>(dto);

			context.Hotels.Add(hotel);

			await context.SaveChangesAsync();

			var hotelDto = mapper.Map<HotelDto>(hotel);

			return hotelDto;
		}

		public async Task<bool> UpdateHotelAsync(Guid id, UpdateHotelDto dto)
		{
			var hotel = await context.Hotels
									 .FirstOrDefaultAsync(h => h.Id == id);

			if (hotel is null)
			{
				return false;
			}

			mapper.Map(dto, hotel);

			await context.SaveChangesAsync();

			return true;
		}

		public async Task<bool> DeleteHotelByIdAsync(Guid id)
		{
			var hotel = await context.Hotels
									 .FirstOrDefaultAsync(h => h.Id == id);

			if (hotel is null)
			{
				return false;
			}

			context.Hotels.Remove(hotel);

			await context.SaveChangesAsync();

			return true;
		}
	}
}