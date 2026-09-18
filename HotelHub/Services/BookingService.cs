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
	public class BookingService(
		ApplicationDbContext context,
		IMapper mapper)
		: IBookingService
	{
		public async Task<BookingDto> CreateAsync(
			Guid roomId,
			Guid userId,
			CreateBookingDto dto)
		{
			// Validate the booking dates.
			// Check-out must be after check-in.
			if (dto.CheckIn >= dto.CheckOut)
			{
				throw new ArgumentException(
					"Check-out date must be after check-in date.");
			}

			// Validate the number of guests.
			if (dto.Guests <= 0)
			{
				throw new ArgumentException(
					"Guests must be greater than zero.");
			}

			// Retrieve the room and its hotel.
			// AsNoTracking is suitable because we only need to read room data.
			var room = await context.Rooms
				.AsNoTracking()
				.Include(r => r.Hotel)
				.FirstOrDefaultAsync(r => r.Id == roomId);

			if (room is null)
			{
				throw new KeyNotFoundException(
					"Room was not found.");
			}

			// Make sure the number of guests does not exceed room capacity.
			if (dto.Guests > room.Capacity)
			{
				throw new ArgumentException(
					$"The room capacity is {room.Capacity} guests.");
			}

			// Check whether the room is already booked for the selected dates.
			//
			// Overlap condition:
			// New CheckIn  < Existing CheckOut
			// New CheckOut > Existing CheckIn
			//
			// Cancelled bookings do not block room availability.
			var hasConflict = await context.Bookings
				.AnyAsync(b =>
					b.RoomId == roomId &&
					b.Status != BookingStatus.Cancelled &&
					dto.CheckIn < b.CheckOut &&
					dto.CheckOut > b.CheckIn);

			if (hasConflict)
			{
				throw new InvalidOperationException(
					"The room is already booked for the selected dates.");
			}

			// Calculate the number of nights.
			// DateOnly.DayNumber makes the calculation straightforward.
			var numberOfNights =
				dto.CheckOut.DayNumber - dto.CheckIn.DayNumber;

			// Calculate the total booking price on the server.
			// The client does not provide the price.
			var totalPrice =
				numberOfNights * room.PricePerNight;

			// Create the Booking entity.
			var booking = new Booking
			{
				RoomId = roomId,
				UserId = userId,
				CheckIn = dto.CheckIn,
				CheckOut = dto.CheckOut,
				Guests = dto.Guests,
				TotalPrice = totalPrice,
				Status = BookingStatus.Pending
			};

			context.Bookings.Add(booking);

			// Save the booking.
			// The database generates the Booking Id.
			await context.SaveChangesAsync();

			// Reload the created booking with its related Room and Hotel.
			// This is required because BookingDto contains RoomNumber and HotelName.
			var createdBooking = await context.Bookings
				.AsNoTracking()
				.Include(b => b.Room)
					.ThenInclude(r => r.Hotel)
				.FirstAsync(b => b.Id == booking.Id);

			return mapper.Map<BookingDto>(createdBooking);
		}
		public async Task<bool> DeleteAsync(
			Guid roomId,
			Guid bookingId,
			Guid userId)
		{
			// Find the booking belonging to the specified room.
			var booking = await context.Bookings
				.FirstOrDefaultAsync(b =>
					b.Id == bookingId &&
					b.RoomId == roomId);

			if (booking is null)
			{
				throw new KeyNotFoundException(
					"Booking was not found.");
			}

			// Only the user who created the booking can cancel it.
			if (booking.UserId != userId)
			{
				throw new UnauthorizedAccessException(
					"You are not allowed to cancel this booking.");
			}

			// Prevent cancelling an already cancelled booking.
			if (booking.Status == BookingStatus.Cancelled)
			{
				throw new InvalidOperationException(
					"Booking is already cancelled.");
			}

			// Soft cancellation:
			// We keep the booking record for historical purposes
			// instead of physically deleting it from the database.
			booking.Status = BookingStatus.Cancelled;
			booking.UpdatedAtUtc = DateTime.UtcNow;

			await context.SaveChangesAsync();

			return true;
		}
		public async Task<PagedResult<BookingDto>> GetAllAsync(
				Guid roomId,
				string? searchTerm,
				BookingFilterDto? filter,
				SortingRequest? sorting,
				int pageNumber,
				int pageSize)
		{
			var roomExists = await context.Rooms
				.AnyAsync(r => r.Id == roomId);

			if (!roomExists)
			{
				throw new KeyNotFoundException(
					"Room was not found.");
			}

			var query = context.Bookings
				.AsNoTracking()
				.Include(b => b.Room)
					.ThenInclude(r => r.Hotel)
				.Where(b => b.RoomId == roomId);

			if (filter is not null)
			{
				query = query
					.WhereIf(
						filter.Status.HasValue,
						b => b.Status == filter.Status!.Value)

					.WhereIf(
						filter.CheckInFrom.HasValue,
						b => b.CheckIn >= filter.CheckInFrom!.Value)

					.WhereIf(
						filter.CheckInTo.HasValue,
						b => b.CheckIn <= filter.CheckInTo!.Value)

					.WhereIf(
						filter.CheckOutFrom.HasValue,
						b => b.CheckOut >= filter.CheckOutFrom!.Value)

					.WhereIf(
						filter.CheckOutTo.HasValue,
						b => b.CheckOut <= filter.CheckOutTo!.Value)

					.WhereIf(
						filter.MinGuests.HasValue,
						b => b.Guests >= filter.MinGuests!.Value)

					.WhereIf(
						filter.MaxGuests.HasValue,
						b => b.Guests <= filter.MaxGuests!.Value);
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
				query = query.OrderByDescending(
					b => b.CheckIn);
			}

			var result = await query.ToPagedResultAsync(
				pageNumber,
				pageSize);

			return result.MapTo<Booking, BookingDto>(mapper);
		}
		public async Task<BookingDto?> GetByIdAsync(
			Guid roomId,
			Guid bookingId)
		{
			// Filtering by both IDs ensures that the booking
			// belongs to the requested room.
			var booking = await context.Bookings
				.AsNoTracking()
				.Include(b => b.Room)
					.ThenInclude(r => r.Hotel)
				.FirstOrDefaultAsync(b =>
					b.RoomId == roomId &&
					b.Id == bookingId);

			if (booking is null)
			{
				throw new KeyNotFoundException(
					"Booking was not found.");
			}

			return mapper.Map<BookingDto>(booking);
		}
		public async Task<bool> UpdateAsync(
			Guid roomId,
			Guid bookingId,
			Guid userId,
			UpdateBookingDto dto)
		{
			// Validate the booking dates.
			if (dto.CheckIn >= dto.CheckOut)
			{
				throw new ArgumentException(
					"Check-out date must be after check-in date.");
			}

			// Retrieve the booking and its related Room.
			// The Room is required to validate capacity and recalculate the price.
			var booking = await context.Bookings
				.Include(b => b.Room)
				.FirstOrDefaultAsync(b =>
					b.Id == bookingId &&
					b.RoomId == roomId);

			if (booking is null)
			{
				throw new KeyNotFoundException(
					"Booking was not found.");
			}

			// Only the owner of the booking can update it.
			if (booking.UserId != userId)
			{
				throw new UnauthorizedAccessException(
					"You are not allowed to update this booking.");
			}

			// Cancelled bookings cannot be modified.
			if (booking.Status == BookingStatus.Cancelled)
			{
				throw new InvalidOperationException(
					"Cancelled bookings cannot be updated.");
			}

			// Validate the number of guests.
			if (dto.Guests <= 0)
			{
				throw new ArgumentException(
					"Guests must be greater than zero.");
			}

			// Make sure the new guest count fits the room capacity.
			if (dto.Guests > booking.Room.Capacity)
			{
				throw new ArgumentException(
					$"The room capacity is {booking.Room.Capacity} guests.");
			}

			// Check for date conflicts with other active bookings.
			// The current booking is excluded from this check.
			var hasConflict = await context.Bookings
				.AnyAsync(b =>
					b.Id != bookingId &&
					b.RoomId == roomId &&
					b.Status != BookingStatus.Cancelled &&
					dto.CheckIn < b.CheckOut &&
					dto.CheckOut > b.CheckIn);

			if (hasConflict)
			{
				throw new InvalidOperationException(
					"The room is already booked for the selected dates.");
			}

			// Calculate the new number of nights.
			var numberOfNights =
				dto.CheckOut.DayNumber - dto.CheckIn.DayNumber;

			// Update only the fields allowed to change.
			// RoomId, UserId, Status, and Id remain unchanged.
			booking.CheckIn = dto.CheckIn;
			booking.CheckOut = dto.CheckOut;
			booking.Guests = dto.Guests;

			// Recalculate the total price using the server-side room price.
			booking.TotalPrice =
				booking.Room.PricePerNight * numberOfNights;

			booking.UpdatedAtUtc = DateTime.UtcNow;

			await context.SaveChangesAsync();

			return true;
		}
	}
}