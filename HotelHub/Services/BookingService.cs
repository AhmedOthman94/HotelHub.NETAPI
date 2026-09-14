using AutoMapper;
using HotelHub.API.Data;
using HotelHub.API.DTOs;
using HotelHub.API.Entity;
using HotelHub.API.Enums;
using HotelHub.API.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace HotelHub.API.Services
{
	public class BookingService (
								ApplicationDbContext context,
								IMapper mapper)
	: IBookingService
	{
		public async Task<BookingDto> CreateAsync(
							Guid hotelId,
							CreateBookingDto dto,
							Guid userId)
		{
			if (hotelId != dto.HotelId)
				throw new ArgumentException(
					"Hotel ID in route does not match Hotel ID in request.");

			var hotel = await context.Hotels
				.AsNoTracking()
				.FirstOrDefaultAsync(h => h.Id == hotelId);

			if (hotel is null)
				throw new KeyNotFoundException("Hotel not found.");

			var hasConflict = await context.Bookings
				.AnyAsync(b =>
					b.HotelId == hotelId &&
					b.Status != BookingStatus.Cancelled &&
					dto.CheckIn < b.CheckOut &&
					dto.CheckOut > b.CheckIn);

			if (hasConflict)
				throw new InvalidOperationException(
					"The hotel is already booked for the selected dates.");

			var numberOfNights =
				dto.CheckOut.DayNumber - dto.CheckIn.DayNumber;

			var totalPrice =
				hotel.PerNight * numberOfNights;

			var booking = new Booking
			{
				HotelId = hotelId,
				UserId = userId,
				CheckIn = dto.CheckIn,
				CheckOut = dto.CheckOut,
				Guests = dto.Guests,
				TotalPrice = totalPrice,
				Status = BookingStatus.Pending
			};

			context.Bookings.Add(booking);

			await context.SaveChangesAsync();

			var createdBooking = await context.Bookings
				.AsNoTracking()
				.Include(b => b.Hotel)
				.FirstAsync(b => b.Id == booking.Id);

			return mapper.Map<BookingDto>(createdBooking);
		}

		public async Task<bool> DeleteAsync(
							Guid hotelId,
							Guid bookingId,
							Guid userId)
		{
			var booking = await context.Bookings
				.FirstOrDefaultAsync(b =>
					b.Id == bookingId &&
					b.HotelId == hotelId);

			if (booking is null)
				return false;

			if (booking.UserId != userId)
				throw new UnauthorizedAccessException(
					"You are not allowed to cancel this booking.");

			if (booking.Status == BookingStatus.Cancelled)
				return false;

			booking.Status = BookingStatus.Cancelled;
			booking.UpdatedAtUtc = DateTime.UtcNow;

			await context.SaveChangesAsync();

			return true;
		}

		public async Task<IEnumerable<BookingDto>> GetAllAsync(
										Guid hotelId)
		{
			var bookings = await context.Bookings
				.AsNoTracking()
				.Include(b => b.Hotel)
				.Where(b => b.HotelId == hotelId)
				.OrderByDescending(b => b.CheckIn)
				.ToListAsync();

			return mapper.Map<IEnumerable<BookingDto>>(bookings);
		}

		public async Task<BookingDto?> GetByIdAsync(
								Guid bookingId,
								Guid hotelId)
		{
			var booking = await context.Bookings
				.AsNoTracking()
				.Include(b => b.Hotel)
				.FirstOrDefaultAsync(b =>
					b.Id == bookingId &&
					b.HotelId == hotelId);

			if (booking is null)
				return null;

			return mapper.Map<BookingDto>(booking);
		}

		public async Task<bool> UpdateAsync(
							Guid hotelId,
							Guid bookingId,
							UpdateBookingDto dto,
							Guid userId)
		{
			var booking = await context.Bookings
				.Include(b => b.Hotel)
				.FirstOrDefaultAsync(b =>
					b.Id == bookingId &&
					b.HotelId == hotelId);

			if (booking is null)
				return false;

			if (booking.UserId != userId)
				throw new UnauthorizedAccessException(
					"You are not allowed to update this booking.");

			if (booking.Status == BookingStatus.Cancelled)
				throw new InvalidOperationException(
					"Cancelled bookings cannot be updated.");

			var hasConflict = await context.Bookings
				.AnyAsync(b =>
					b.Id != bookingId &&
					b.HotelId == hotelId &&
					b.Status != BookingStatus.Cancelled &&
					dto.CheckIn < b.CheckOut &&
					dto.CheckOut > b.CheckIn);

			if (hasConflict)
				throw new InvalidOperationException(
					"The hotel is already booked for the selected dates.");

			var numberOfNights =
				dto.CheckOut.DayNumber - dto.CheckIn.DayNumber;

			booking.CheckIn = dto.CheckIn;
			booking.CheckOut = dto.CheckOut;
			booking.Guests = dto.Guests;

			booking.TotalPrice =
				booking.Hotel.PerNight * numberOfNights;

			booking.UpdatedAtUtc = DateTime.UtcNow;

			await context.SaveChangesAsync();

			return true;
		}
	}
}
