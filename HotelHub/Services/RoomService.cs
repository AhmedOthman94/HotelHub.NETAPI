using AutoMapper;
using HotelHub.API.Data;
using HotelHub.API.DTOs;
using HotelHub.API.Entity;
using HotelHub.API.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace HotelHub.API.Services
{
	public class RoomService (
					ApplicationDbContext context,
					IMapper mapper)
	: IRoomService
	{
		public async Task<RoomDto> CreateAsync(Guid hotelId, CreateRoomDto dto)
		{
			var hotelExists = await context.Hotels
										.AnyAsync(h => h.Id == hotelId);
			if(!hotelExists)
			{
				throw new KeyNotFoundException(
								"Hotel was not found.");
			}

			var room = mapper.Map<Room>(dto);
			room.HotelId = hotelId;

			context.Rooms.Add(room);
			await context.SaveChangesAsync();

			return mapper.Map<RoomDto>(room);
		}

		public async Task<bool> DeleteAsync(Guid hotelId, Guid roomId)
		{
			var hotelExists = await context.Hotels
										.AnyAsync(h => h.Id == hotelId);

			if (!hotelExists)
			{
				throw new KeyNotFoundException(
							"Hotel was not found.");
			}

			var room = await context.Rooms
								.FirstOrDefaultAsync(r => 
									r.HotelId == hotelId && 
									r.Id == roomId);

			if (room is null)
			{
				throw new KeyNotFoundException(
								"Room was not found.");
			}

			context.Rooms.Remove(room);
			await context.SaveChangesAsync();

			return true;
		}

		public async Task<IEnumerable<RoomDto>> GetAllAsync(Guid hotelId)
		{
			var hotelExists = await context.Hotels
										.AnyAsync(h => h.Id == hotelId);

			if (!hotelExists)
			{
				throw new KeyNotFoundException(
					"Hotel was not found.");
			}

			var rooms = await context.Rooms
								.AsNoTracking()
								.Where(r => r.HotelId == hotelId)
								.OrderBy(r => r.RoomNumber)
								.ToListAsync();

			return mapper.Map<IEnumerable<RoomDto>>(rooms);
		}

		public async Task<RoomDto?> GetByIdAsync(Guid hotelId, Guid roomId)
		{
			var hotelExists = await context.Hotels
								.AnyAsync(h => h.Id == hotelId);

			if (!hotelExists)
			{
				throw new KeyNotFoundException(
							"Hotel was not found.");
			}

			var room = await context.Rooms
								.AsNoTracking()
								.FirstOrDefaultAsync(r => r.HotelId == hotelId &&
									r.Id == roomId);

			if (room is null)
			{
				throw new KeyNotFoundException(
								"Room was not found.");
			}

			return mapper.Map<RoomDto>(room);
		}

		public async Task<bool> UpdateAsync(Guid hotelId, Guid roomId, UpdateRoomDto dto)
		{
			var hotelExists = await context.Hotels
										.AnyAsync(h => h.Id == hotelId);
			if (!hotelExists)
			{
				throw new KeyNotFoundException(
							"Hotel was not found.");
			}

			var room = await context.Rooms
								.FirstOrDefaultAsync( r => 
									r.HotelId == hotelId && 
									r.Id == roomId);

			if (room is null)
			{
				throw new KeyNotFoundException(
							"Room was not found.");
			}

			mapper.Map(dto, room);
			await context.SaveChangesAsync();

			return true;
		}
	}
}
