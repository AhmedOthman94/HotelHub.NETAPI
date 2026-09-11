using AutoMapper;
using HotelHub.API.Data;
using HotelHub.API.DTOs;
using HotelHub.API.Entity;
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

		public async Task<IEnumerable<HotelDto>> GetAllHotelsAsync()
		{
			var hotels = await context.Hotels
									   .AsNoTracking()
									   .Include(h => h.Country)
									   .OrderBy(h => h.Name)
									   .ToListAsync();

			var hotelsDto = mapper.Map<IEnumerable<HotelDto>>(hotels);

			return hotelsDto;
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