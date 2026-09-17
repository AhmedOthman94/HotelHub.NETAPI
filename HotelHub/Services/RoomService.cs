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

		public async Task<PagedResult<RoomDto>> GetAllAsync(
					Guid hotelId,
					string? searchTerm,
					RoomFilterDto? filter,
					SortingRequest? sorting,
					int pageNumber,
					int pageSize)
		{
			var hotelExists = await context.Hotels
				.AnyAsync(h => h.Id == hotelId);

			if (!hotelExists)
			{
				throw new KeyNotFoundException(
					"Hotel was not found.");
			}

			var query = context.Rooms
				.AsNoTracking()
				.Where(r => r.HotelId == hotelId)
				.Search(
					searchTerm,
					r => r.RoomNumber);

			if (filter is not null)
			{
				query = query
					.WhereIf(
						filter.RoomType.HasValue,
						r => r.RoomType == filter.RoomType!.Value)

					.WhereIf(
						filter.MinCapacity.HasValue,
						r => r.Capacity >= filter.MinCapacity!.Value)

					.WhereIf(
						filter.MaxCapacity.HasValue,
						r => r.Capacity <= filter.MaxCapacity!.Value)

					.WhereIf(
						filter.MinPrice.HasValue,
						r => r.PricePerNight >= filter.MinPrice!.Value)

					.WhereIf(
						filter.MaxPrice.HasValue,
						r => r.PricePerNight <= filter.MaxPrice!.Value);
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
				query = query.OrderBy(
					r => r.RoomNumber);
			}

			var result = await query.ToPagedResultAsync(
				pageNumber,
				pageSize);

			return result.MapTo<Room, RoomDto>(mapper);
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
